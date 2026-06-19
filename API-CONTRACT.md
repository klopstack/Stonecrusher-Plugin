# Moonfin plugin API contract (Stonecrusher Android TV)

This document catalogs `/Moonfin/*` routes and settings keys that
[Stonecrusher Media Android TV](https://github.com/klopstack/StonecrusherMedia-AndroidTV)
depends on. Changes here are **breaking** for the TV client unless a matching
client release ships at the same time.

Source of truth on the client side: `PluginSyncConstants.kt`, `PluginSyncService.kt`,
and the repository classes listed below.

## Settings sync

| Route | Method | Client |
|-------|--------|--------|
| `/Moonfin/Ping` | GET | `PluginSyncService.kt` |
| `/Moonfin/Settings` | GET | `PluginSyncService.kt` |
| `/Moonfin/Settings` | POST | `PluginSyncService.kt` (v1 flat settings) |
| `/Moonfin/Settings/Profile/global` | POST | `PluginSyncService.kt` (v2 profile envelope) |

### Synced settings keys (v1 / global profile)

These camelCase keys must remain supported in the plugin settings model:

| Server key | Client preference |
|------------|-------------------|
| `navbarPosition` | `UserPreferences.navbarPosition` |
| `showShuffleButton` | `UserPreferences.showShuffleButton` |
| `showGenresButton` | `UserPreferences.showGenresButton` |
| `showFavoritesButton` | `UserPreferences.showFavoritesButton` |
| `showLibrariesInToolbar` | `UserPreferences.showLibrariesInToolbar` |
| `shuffleContentType` | `UserPreferences.shuffleContentType` |
| `mergeContinueWatchingNextUp` | `UserPreferences.mergeContinueWatchingNextUp` |
| `enableMultiServerLibraries` | `UserPreferences.enableMultiServerLibraries` |
| `enableFolderView` | `UserPreferences.enableFolderView` |
| `confirmExit` | `UserPreferences.confirmExit` |
| `seasonalSurprise` | `UserPreferences.seasonalSurprise` |
| `mediaBarEnabled` | `UserSettingPreferences.mediaBarEnabled` |
| `mediaBarSourceType` | `UserSettingPreferences.mediaBarSourceType` |
| `mediaBarContentType` | `UserSettingPreferences.mediaBarContentType` |
| `mediaBarItemCount` | `UserSettingPreferences.mediaBarItemCount` |
| `mediaBarExcludedGenres` | `UserSettingPreferences.mediaBarExcludedGenres` |
| `mediaBarOpacity` | `UserSettingPreferences.mediaBarOverlayOpacity` |
| `mediaBarOverlayColor` | `UserSettingPreferences.mediaBarOverlayColor` |
| `themeMusicEnabled` | `UserSettingPreferences.themeMusicEnabled` |
| `themeMusicVolume` | `UserSettingPreferences.themeMusicVolume` |
| `themeMusicOnHomeRows` | `UserSettingPreferences.themeMusicOnHomeRows` |
| `homeRowsImageTypeOverride` | `UserSettingPreferences.homeRowsUniversalOverride` |
| `homeRowsImageType` | `UserSettingPreferences.homeRowsUniversalImageType` |
| `detailsScreenBlur` | `UserSettingPreferences.detailsBackgroundBlurAmount` |
| `browsingBlur` | `UserSettingPreferences.browsingBackgroundBlurAmount` |
| `mdblistEnabled` | `UserSettingPreferences.enableAdditionalRatings` |
| `tmdbEpisodeRatingsEnabled` | `UserSettingPreferences.enableEpisodeRatings` |
| `userPinEnabled` | `UserSettingPreferences.userPinEnabled` |
| `userPinHash` | `UserSettingPreferences.userPinHash` |
| `userPinSetupDeclined` | `UserSettingPreferences.userPinSetupDeclined` |
| `jellyseerrEnabled` | `JellyseerrPreferences.enabled` |
| `jellyseerrApiKey` | `JellyseerrPreferences.apiKey` |
| `jellyseerrBlockNsfw` | `JellyseerrPreferences.blockNsfw` |

> **Note:** `userPinHash`, `userPinEnabled`, and `userPinSetupDeclined` are stored
> per authenticated Jellyfin user. The TV client reads/writes them from per-user
> local storage; the plugin must persist them per user on the server.

## Jellyseerr / Seerr proxy

| Route | Client |
|-------|--------|
| `/Moonfin/Jellyseerr/Config` | `PluginSyncService.kt`, `JellyseerrRepository.kt` |
| `/Moonfin/Jellyseerr/Api/*` | `JellyseerrHttpClient.kt` |

## OTA client updates (GitHub flavor)

| Route | Client |
|-------|--------|
| `/Moonfin/ClientUpdate` | `UpdateCheckerService.kt` |

## Media bar

| Route | Client |
|-------|--------|
| `/Moonfin/MediaBar` | `MediaBarSlideshowViewModel.kt` |
| `/Moonfin/Genres` | `SettingsMoonfinMediaBarExcludedGenresScreen.kt` |

## Ratings

| Route | Client |
|-------|--------|
| `/Moonfin/Tmdb/*` | `TmdbRepository.kt` |
| `/Moonfin/MdbList/Ratings` | `MdbListRepository.kt` |
| `/Moonfin/Assets/*` | `RatingIconProvider.kt` |

## Versioning

Until this fork publishes its own releases, the baseline compatible upstream
plugin version is **1.9.1**. Pin tested client ↔ plugin pairs in release notes
when this fork begins shipping builds.
