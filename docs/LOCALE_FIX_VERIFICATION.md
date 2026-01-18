# Locale Loading Fix - Manual Verification Guide

## Issue Fixed
Default locale file not loaded on initial app load, causing translation keys to be displayed instead of localized text.

## How to Verify the Fix

### Test Scenario 1: First Time Visit (No localStorage)
1. Open browser in **incognito/private mode** (to ensure no localStorage data)
2. Navigate to https://storingsdienst.jll.io/
3. **Expected Result**: Dutch text should appear immediately (not "Step1SelectDataSource", etc.)
4. Open browser DevTools → Console
5. **Expected Log Output**:
   ```
   [LocalizationService] InitializeAsync called
   [LocalizationService] Loading translations from JSON files...
   [LocalizationService] Loaded XXX Dutch translations
   [LocalizationService] Loaded XXX English translations
   [LocalizationService] savedCulture from localStorage: 'null'
   [LocalizationService] _currentCulture before: 'nl'
   [LocalizationService] Translations loaded, notifying subscribers
   [LocalizationService] _currentCulture after: 'nl'
   ```

### Test Scenario 2: Return Visit with Saved Preference (The Bug Case)
1. Visit https://storingsdienst.jll.io/ in a **normal browser session**
2. Select a language (e.g., click the Dutch or English flag)
3. Refresh the page (F5 or Ctrl+R)
4. **Expected Result**: Text should still appear in the selected language immediately
5. Open browser DevTools → Console

#### Scenario 2a: Saved preference matches default (nl)
**Expected Log Output**:
```
[LocalizationService] InitializeAsync called
[LocalizationService] Loading translations from JSON files...
[LocalizationService] Loaded XXX Dutch translations
[LocalizationService] Loaded XXX English translations
[LocalizationService] savedCulture from localStorage: 'nl'
[LocalizationService] _currentCulture before: 'nl'
[LocalizationService] Setting language to: 'nl'
[LocalizationService] SetLanguageInternalAsync called with culture='nl', persist=False
[LocalizationService] Culture already set to 'nl', returning early
[LocalizationService] Translations loaded, notifying subscribers  ← KEY LINE!
[LocalizationService] _currentCulture after: 'nl'
```

#### Scenario 2b: Saved preference differs from default (en)
**Expected Log Output**:
```
[LocalizationService] InitializeAsync called
[LocalizationService] Loading translations from JSON files...
[LocalizationService] Loaded XXX Dutch translations
[LocalizationService] Loaded XXX English translations
[LocalizationService] savedCulture from localStorage: 'en'
[LocalizationService] _currentCulture before: 'nl'
[LocalizationService] Setting language to: 'en'
[LocalizationService] SetLanguageInternalAsync called with culture='en', persist=False
[LocalizationService] Changing culture from 'nl' to 'en'
[LocalizationService] Set DefaultThreadCurrentUICulture to 'en'
[LocalizationService] Invoking OnLanguageChanged event  ← Event fires here
[LocalizationService] OnLanguageChanged event completed
[LocalizationService] _currentCulture after: 'en'
```
**Note**: When culture differs from default, `SetLanguageInternalAsync` fires the event, so the "Translations loaded, notifying subscribers" log does NOT appear (preventing duplicate event firing).

### Test Scenario 3: Language Switching
1. Visit the app
2. Click the language selector (Dutch 🇳🇱 or English 🇬🇧 flag)
3. **Expected Result**: Language changes immediately
4. **Expected Log Output**:
   ```
   [LocalizationService] SetLanguageInternalAsync called with culture='en', persist=True
   [LocalizationService] Changing culture from 'nl' to 'en'
   [LocalizationService] Set DefaultThreadCurrentUICulture to 'en'
   [LocalizationService] Persisted culture 'en' to localStorage
   [LocalizationService] Invoking OnLanguageChanged event
   [LocalizationService] OnLanguageChanged event completed
   ```

## What Was Broken Before

**Symptom**: On return visits with saved locale preference matching the default ("nl"), users saw translation keys like "Step1SelectDataSource" instead of "Stap 1: Selecteer gegevensbron".

**Root Cause**: When `savedCulture` matched `_currentCulture` (both "nl"), the code flow was:
1. Load translations ✅
2. Try to set culture to "nl"
3. Early return because culture is already "nl" (no event fired) ❌
4. Skip `else if (justLoaded)` block because first `if` was true
5. Components never notified to re-render

**Fix**: Changed logic to fire the `OnLanguageChanged` event when translations are just loaded **only if** the culture didn't change (preventing duplicate event firing when culture changes).

## Code Change Summary

**File**: `src/Storingsdienst/Storingsdienst.Client/Services/LocalizationService.cs`

**Before** (lines 85-97):
```csharp
if (!string.IsNullOrEmpty(savedCulture) &&
    (savedCulture == "nl" || savedCulture == "en"))
{
    await SetLanguageInternalAsync(savedCulture, persist: false);
}
else if (justLoaded)  // ← Only fires if savedCulture is empty/invalid
{
    OnLanguageChanged?.Invoke();
}
```

**After** (lines 85-103):
```csharp
bool cultureChanged = false;
if (!string.IsNullOrEmpty(savedCulture) &&
    (savedCulture == "nl" || savedCulture == "en"))
{
    var previousCulture = _currentCulture;
    await SetLanguageInternalAsync(savedCulture, persist: false);
    cultureChanged = previousCulture != _currentCulture;
}

// Notify subscribers when translations are just loaded AND culture didn't change
// (if culture changed, SetLanguageInternalAsync already fired the event)
if (justLoaded && !cultureChanged)  // ← Prevents duplicate event firing
{
    OnLanguageChanged?.Invoke();
}
```

## Impact
- **Users affected**: Anyone revisiting the app with Dutch (default) language preference saved
- **Severity**: High (broken UI, confusing UX)
- **Fix complexity**: Low (minimal code change with culture change detection)
- **Risk**: Very low (well-tested locale loading logic with comprehensive test coverage, backward compatible)
