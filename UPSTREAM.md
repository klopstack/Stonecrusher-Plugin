# Upstream tracking

This repository is a maintained fork of the Moonfin Jellyfin server plugin for
[Stonecrusher Media Android TV](https://github.com/klopstack/StonecrusherMedia-AndroidTV).

Tracking issue: [StonecrusherMedia-AndroidTV#54](https://github.com/klopstack/StonecrusherMedia-AndroidTV/issues/54)

## Upstream source

| Item | Value |
|------|-------|
| Repository | [Moonfin-Client/Plugin](https://github.com/Moonfin-Client/Plugin) |
| License | GPLv3 (preserved in `LICENSE`) |
| Default branch | `master` |

## Fork baseline

This fork was created from upstream at:

| Item | Value |
|------|-------|
| Commit | `6ac5b0853f9a90b6ed1f44929d4e2f44e431a11f` |
| Commit date | 2026-06-17 |
| Commit message | autosave when applying settings to users (#149) |
| Latest upstream tag at fork time | `1.9.1` (`22938780427e12cb6ec9544b2d44fc416695d278`) |

Record new upstream sync points here when cherry-picking or merging from
`Moonfin-Client/Plugin`.

## Remotes

```bash
git remote add upstream https://github.com/Moonfin-Client/Plugin.git
git fetch upstream
```

## Moonfin-Core dependency

The plugin's Flutter web UI (`frontend/`) is built from the separate
[Moonfin-Client/Moonfin-Core](https://github.com/Moonfin-Client/Moonfin-Core)
repository and synced into this repo at release time.

**Evaluation (2026-06-19):** Moonfin-Core does **not** need to be forked yet for
Stonecrusher Android TV. The TV client depends on the plugin's `/Moonfin/*` HTTP
API and settings sync, not the bundled web app. Revisit if we need to ship custom
web UI changes from this fork.

## API contract

The Stonecrusher Android TV client is tightly coupled to this plugin's HTTP API.
See `API-CONTRACT.md` for the routes and settings keys the client depends on.

Treat changes to those routes or sync keys as **breaking** and release coordinated
client + plugin updates.

## Sync workflow

1. `git fetch upstream`
2. Review upstream changes for API/schema impact against `API-CONTRACT.md`
3. Cherry-pick or merge selected commits — do not blindly fast-forward
4. Update this file with the new upstream baseline when syncing
5. Run plugin CI (`.github/workflows/build.yml`) and coordinate Android TV client testing before release
6. Publish fork builds with a Stonecrusher tag (for example `1.9.1-sc1`); see README *Fork CI and releases*

## Release automation (manifest commits)

The [Release workflow](.github/workflows/release.yml) commits an updated `manifest.json`
to `master` after each `*-sc*` tag push, uploads release assets with MD5/SHA256
checksums, and publishes `repository.json` to the `gh-pages` branch via
[`jellyfin-plugin-repo-action`](https://github.com/Kevinjil/jellyfin-plugin-repo-action)
(the same pattern used by [auto-parental-tags](https://github.com/klopstack/auto-parental-tags)
and [Jellyfin.Xtream](https://github.com/klopstack/Jellyfin.Xtream)).

Jellyfin plugin repository URL:

`https://klopstack.github.io/Stonecrusher-Plugin/repository.json`

### One-time GitHub Pages setup

After the first catalog publish workflow succeeds:

1. Open **Settings → Pages** on `klopstack/Stonecrusher-Plugin`.
2. Set **Source** to **Deploy from a branch**, branch **`gh-pages`**, folder **`/ (root)`**.
3. Save. The catalog URL above should return JSON within a few minutes.

To republish the catalog without a new release, run the
[Publish Plugin Catalog](.github/workflows/publish-catalog.yml) workflow manually.

### GitHub App setup (manifest commits)

1. Create a [GitHub App](https://docs.github.com/en/apps/creating-github-apps) with
   **Contents: Read and write** permission.
2. Install the app on `klopstack/Stonecrusher-Plugin`.
3. Add repository secrets:
   - `RELEASE_APP_ID` — the app ID (numeric)
   - `RELEASE_APP_PRIVATE_KEY` — the app's PEM private key
4. Add the GitHub App to the **Protect Main** ruleset bypass list with **Always allow**:
   [settings/rules/17875646](https://github.com/klopstack/Stonecrusher-Plugin/settings/rules/17875646)

Without step 4, `git push origin master` in the release workflow will still fail even
with a valid app token.
