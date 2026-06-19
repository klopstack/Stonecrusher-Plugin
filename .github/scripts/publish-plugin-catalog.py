#!/usr/bin/env python3
"""Generate Jellyfin plugin repository.json and commit it to gh-pages.

Compatible with Kevinjil/jellyfin-plugin-repo-action, but tolerates release tags
that predate build.yaml by falling back to master build.yaml and manifest data.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import os
import re
import sys
from typing import Any
from urllib.error import HTTPError
from urllib.request import Request, urlopen

import yaml


def github_request(
    token: str,
    method: str,
    path: str,
    *,
    data: dict[str, Any] | None = None,
) -> Any:
    url = f"https://api.github.com{path}"
    body = None if data is None else json.dumps(data).encode("utf-8")
    request = Request(
        url,
        data=body,
        method=method,
        headers={
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "stonecrusher-plugin-catalog",
        },
    )
    with urlopen(request) as response:
        payload = response.read()
        if not payload:
            return None
        return json.loads(payload)


def download_text(url: str) -> str:
    request = Request(url, headers={"User-Agent": "stonecrusher-plugin-catalog"})
    with urlopen(request) as response:
        return response.read().decode("utf-8")


def get_repo_file(token: str, owner: str, repo: str, path: str, ref: str | None = None) -> str | None:
    query = f"?ref={ref}" if ref else ""
    try:
        payload = github_request(token, "GET", f"/repos/{owner}/{repo}/contents/{path}{query}")
    except HTTPError as error:
        if error.code == 404:
            return None
        raise
    assert isinstance(payload, dict)
    return base64.b64decode(payload["content"]).decode("utf-8")


def parse_yaml(text: str) -> dict[str, Any]:
    parsed = yaml.safe_load(text)
    if not isinstance(parsed, dict):
        raise ValueError("Expected YAML mapping")
    return parsed


def catalog_version_from_tag(tag: str, assembly_version: str | None = None) -> str:
    match = re.fullmatch(r"(\d+\.\d+\.\d+)-sc(\d+)", tag)
    if match:
        return f"{match.group(1)}.{match.group(2)}"
    if assembly_version:
        suffix = tag.rsplit("-sc", 1)[-1] if "-sc" in tag else "1"
        parts = assembly_version.split(".")
        if len(parts) >= 3:
            return f"{parts[0]}.{parts[1]}.{parts[2]}.{suffix}"
    return tag


def version_sort_key(version: str) -> tuple[int, ...]:
    parts: list[int] = []
    for part in version.split("."):
        try:
            parts.append(int(part))
        except ValueError:
            parts.append(0)
    return tuple(parts)


def checksum_from_manifest(manifest_text: str | None, version: str) -> str:
    if not manifest_text:
        return ""
    manifest = json.loads(manifest_text)
    if not manifest:
        return ""
    for entry in manifest[0].get("versions", []):
        if entry.get("version") == version:
            return str(entry.get("checksum", "")).upper()
    return ""


def commit_file(
    token: str,
    owner: str,
    repo: str,
    branch: str,
    path: str,
    message: str,
    content: str,
) -> None:
    existing = None
    try:
        existing = github_request(
            token,
            "GET",
            f"/repos/{owner}/{repo}/contents/{path}?ref={branch}",
        )
    except HTTPError as error:
        if error.code != 404:
            raise

    payload: dict[str, Any] = {
        "message": message,
        "content": base64.b64encode(content.encode("utf-8")).decode("ascii"),
        "branch": branch,
        "committer": {
            "name": "github-actions[bot]",
            "email": "41898282+github-actions[bot]@users.noreply.github.com",
        },
        "author": {
            "name": "github-actions[bot]",
            "email": "41898282+github-actions[bot]@users.noreply.github.com",
        },
    }
    if isinstance(existing, dict) and existing.get("sha"):
        payload["sha"] = existing["sha"]

    github_request(token, "PUT", f"/repos/{owner}/{repo}/contents/{path}", data=payload)


def build_repository(
    token: str,
    owner: str,
    repo: str,
    *,
    ignore_prereleases: bool,
) -> list[dict[str, Any]]:
    master_build_text = get_repo_file(token, owner, repo, "build.yaml", "master")
    if master_build_text is None:
        raise RuntimeError("build.yaml is missing from master")
    build_config = parse_yaml(master_build_text)

    manifest_text = get_repo_file(token, owner, repo, "manifest.json", "master")
    versions: list[dict[str, Any]] = []
    releases = github_request(token, "GET", f"/repos/{owner}/{repo}/releases")
    assert isinstance(releases, list)

    for release in releases:
        if release.get("draft"):
            continue
        if release.get("prerelease") and ignore_prereleases:
            continue

        tag = release["tag_name"]
        release_config: dict[str, Any] | None = None
        checksum = ""
        source_url = ""

        for asset in release.get("assets", []):
            name = asset["name"]
            if name.endswith(".zip"):
                source_url = asset["browser_download_url"]
            elif name.endswith(".md5"):
                checksum = download_text(asset["browser_download_url"])[:32].upper()
            elif name == "build.yaml":
                release_config = parse_yaml(download_text(asset["browser_download_url"]))

        if release_config is None:
            tag_build_text = get_repo_file(token, owner, repo, "build.yaml", tag)
            if tag_build_text is not None:
                release_config = parse_yaml(tag_build_text)

        if release_config is None:
            release_config = dict(build_config)
            version = catalog_version_from_tag(tag, str(build_config.get("version", "")))
            release_config["version"] = version
            if not checksum:
                checksum = checksum_from_manifest(manifest_text, version)

        if not checksum and source_url:
            zip_bytes = urlopen(
                Request(source_url, headers={"User-Agent": "stonecrusher-plugin-catalog"})
            ).read()
            checksum = hashlib.md5(zip_bytes).hexdigest().upper()

        versions.append(
            {
                "changelog": release.get("body") or "",
                "checksum": checksum,
                "sourceUrl": source_url,
                "targetAbi": release_config.get("targetAbi", build_config.get("targetAbi", "")),
                "timestamp": release.get("published_at") or "",
                "version": release_config.get("version", build_config.get("version", "")),
            }
        )

    versions.sort(
        key=lambda item: version_sort_key(str(item["version"])),
        reverse=True,
    )

    plugin: dict[str, Any] = {
        "category": build_config.get("category", ""),
        "description": build_config.get("description", ""),
        "guid": build_config.get("guid", ""),
        "name": build_config.get("name", ""),
        "overview": build_config.get("overview", ""),
        "owner": build_config.get("owner", ""),
        "versions": versions,
    }
    if build_config.get("imageUrl"):
        plugin["imageUrl"] = build_config["imageUrl"]

    return [plugin]


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repository", required=True, help="owner/repo")
    parser.add_argument("--pages-branch", default="gh-pages")
    parser.add_argument("--pages-file", default="repository.json")
    parser.add_argument("--ignore-prereleases", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    token = os.environ.get("GITHUB_TOKEN")
    if not token:
        print("GITHUB_TOKEN is required", file=sys.stderr)
        return 1

    owner, repo = args.repository.split("/", 1)
    repository = build_repository(
        token,
        owner,
        repo,
        ignore_prereleases=args.ignore_prereleases,
    )
    content = json.dumps(repository, indent=2, sort_keys=True) + "\n"

    if args.dry_run:
        print(content)
        return 0

    commit_file(
        token,
        owner,
        repo,
        args.pages_branch,
        args.pages_file,
        "Regenerate Jellyfin plugin repository.",
        content,
    )
    print(f"Published {args.pages_file} to {args.pages_branch}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
