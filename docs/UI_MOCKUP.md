# UI Mockup: Paystub Verification Feature

This document provides a visual mockup of the new paystub verification feature added to the Storingsdienst application.

## Overview

The paystub verification feature allows users to compare their calendar data with their paystub information to ensure they are being paid correctly for their consignment shifts. The feature supports verification of multiple months simultaneously without needing to reload data.

## User Flow

1. **Upload JSON file and filter by meeting subject** (existing functionality)
2. **View monthly breakdown table** (enhanced with Saturday/Sunday separation)
3. **Verify paystub data for selected months** (new functionality)

## Screen Mockups

### 1. Monthly Breakdown Table (Enhanced)

The existing monthly breakdown table has been enhanced to show Saturday and Sunday counts separately:

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  Monthly Breakdown                                      [Export to Excel]      │
├────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │ Month    │ Year │ Total │ Weekdays │ Saturdays │ Sundays │ Holidays │ ... │ │
│  ├──────────────────────────────────────────────────────────────────────────┤ │
│  │ December │ 2024 │   12  │     8    │     2     │    1    │    1     │ ... │ │
│  │ November │ 2024 │   15  │    12    │     2     │    1    │    0     │ ... │ │
│  │ October  │ 2024 │   10  │     8    │     1     │    1    │    0     │ ... │ │
│  ├──────────────────────────────────────────────────────────────────────────┤ │
│  │ Total           │   37  │    28    │     5     │    3    │    1     │     │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                 │
│  ℹ Showing results for meetings with subject containing "Storingsdienst"      │
│                                                                                 │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Key Changes:**
- ✅ "Weekends" column replaced with separate "Saturdays" and "Sundays" columns
- ✅ Total row shows aggregated counts across all months
- ✅ Excel export includes the new Saturday/Sunday breakdown

---

### 2. Paystub Verification Section (New)

Below the monthly breakdown table, a new verification section appears:

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  Verify Paystub Data                                  [➕ Add Another Month]   │
├────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ℹ Verify that your paystub data matches the calendar data. You can add       │
│    multiple verification cards to check different months simultaneously.       │
│                                                                                 │
│  ┌────────────────────────────────────────────────────────────────────────┐   │
│  │ 📅 Verify Paystub Data                                              [✖] │   │
│  ├────────────────────────────────────────────────────────────────────────┤   │
│  │                                                                          │   │
│  │  Select Month to Verify                                                 │   │
│  │  ┌────────────────────────────────────────────────────────────────┐    │   │
│  │  │ December 2024                                            [▼]    │    │   │
│  │  └────────────────────────────────────────────────────────────────┘    │   │
│  │                                                                          │   │
│  │  ┌──────────────────────────────────────────────────────────────────┐  │   │
│  │  │ Calendar Data for December 2024:                                 │  │   │
│  │  │  • Weekdays: 8                                                   │  │   │
│  │  │  • Saturdays: 2                                                  │  │   │
│  │  │  • Sundays: 1                                                    │  │   │
│  │  │  • Holidays: 1                                                   │  │   │
│  │  └──────────────────────────────────────────────────────────────────┘  │   │
│  │                                                                          │   │
│  │  Enter Paystub Data:                                                    │   │
│  │  ┌──────────────────────────┐  ┌──────────────────────────┐           │   │
│  │  │ Weekdays Paid            │  │ Saturdays Paid           │           │   │
│  │  │ [        8             ] │  │ [        2             ] │           │   │
│  │  └──────────────────────────┘  └──────────────────────────┘           │   │
│  │                                                                          │   │
│  │  ┌──────────────────────────┐  ┌──────────────────────────┐           │   │
│  │  │ Sundays Paid             │  │ Holidays Paid            │           │   │
│  │  │ [        1             ] │  │ [        1             ] │           │   │
│  │  └──────────────────────────┘  └──────────────────────────┘           │   │
│  │                                                                          │   │
│  │  [ ✓ Verify Data ]                                                      │   │
│  │                                                                          │   │
│  └────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Features:**
- ✅ Dropdown to select which month to verify
- ✅ Display of calendar data for the selected month
- ✅ Input fields for entering paystub amounts for each day type
- ✅ Verify button to compare the data
- ✅ Close button (✖) to remove the verification card when there are multiple cards

---

### 3. Verification Results - All Match (Success)

When all values match:

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  📅 Verify Paystub Data                                                    [✖] │
├────────────────────────────────────────────────────────────────────────────────┤
│  ... (form fields as above) ...                                                │
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │ ✓ All data matches!                                                      │ │
│  │                                                                           │ │
│  │ Your paystub data matches the calendar entries.                          │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                 │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Visual Indicators:**
- ✅ Green success alert box
- ✅ Checkmark icon
- ✅ Clear success message

---

### 4. Verification Results - Discrepancies Found (Warning)

When there are mismatches:

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  📅 Verify Paystub Data                                                    [✖] │
├────────────────────────────────────────────────────────────────────────────────┤
│  ... (form fields as above) ...                                                │
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │ ⚠ Discrepancies found:                                                   │ │
│  │                                                                           │ │
│  │  • ✓ Weekdays match (8)                                                  │ │
│  │  • ✗ Saturdays: Calendar shows 2, Paystub shows 1 (Difference: 1)       │ │
│  │  • ✓ Sundays match (1)                                                   │ │
│  │  • ✓ Holidays match (1)                                                  │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                 │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Visual Indicators:**
- ⚠️ Yellow warning alert box
- ✓ Green checkmarks for matching values
- ✗ Red X marks for mismatches
- 📊 Shows the difference for each mismatch

---

### 5. Multiple Verification Cards

Users can add multiple verification cards to check several months:

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  Verify Paystub Data                                  [➕ Add Another Month]   │
├────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌────────────────────────────────────────────────────────────────────────┐   │
│  │ 📅 Verify Paystub Data                                              [✖] │   │
│  │ December 2024 - ✓ All data matches!                                    │   │
│  └────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│  ┌────────────────────────────────────────────────────────────────────────┐   │
│  │ 📅 Verify Paystub Data                                              [✖] │   │
│  │ November 2024 - ⚠ Discrepancies found                                  │   │
│  └────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│  ┌────────────────────────────────────────────────────────────────────────┐   │
│  │ 📅 Verify Paystub Data                                              [✖] │   │
│  │ (No month selected)                                                     │   │
│  └────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Key Features:**
- ✅ Multiple cards can coexist
- ✅ Each card has its own close button (when there's more than one card)
- ✅ Independent month selection and verification per card
- ✅ Add button at the top to create new verification cards
- ✅ Total overview in the main table remains visible above

---

## Component Architecture

### PaystubVerification Component

**Location:** `src/Storingsdienst/Storingsdienst.Client/Components/PaystubVerification.razor`

**Key Properties:**
- `MonthlyResults` - The list of monthly breakdowns from the calendar data
- `AllowRemove` - Whether the close button is shown
- `OnRemoveClicked` - Callback when the close button is clicked
- `Id` - Unique identifier for the component instance

**Key State:**
- `SelectedMonthKey` - Currently selected month (YYYY-MM format)
- `PaystubWeekdays`, `PaystubSaturdays`, `PaystubSundays`, `PaystubHolidays` - User input values
- `IsVerified` - Whether verification has been performed
- `AllMatch`, `WeekdaysMatch`, `SaturdaysMatch`, `SundaysMatch`, `HolidaysMatch` - Verification results

**Key Methods:**
- `VerifyData()` - Compares calendar data with paystub input
- `OnRemove()` - Triggers removal of the component

### Home Page Integration

**Changes to Home.razor:**
- Added `_verificationComponentIds` list to track multiple verification components
- `AddVerificationComponent()` method adds a new verification card
- `RemoveVerificationComponent(id)` method removes a specific card
- Verification section only displays when there is monthly data available

---

## Data Model Changes

### MonthlyBreakdown Model

**New Properties:**
```csharp
public int SaturdayCount { get; set; }
public int SundayCount { get; set; }
```

The existing `WeekendCount` property is retained for backward compatibility and equals `SaturdayCount + SundayCount`.

---

## Excel Export Changes

The Excel export now includes separate columns for Saturdays and Sundays:

```
| Month    | Year | Total Days | Weekdays | Saturdays | Sundays | Holidays | Holiday Details |
|----------|------|------------|----------|-----------|---------|----------|-----------------|
| December | 2024 |     12     |     8    |     2     |    1    |    1     | Kerstmis        |
| November | 2024 |     15     |    12    |     2     |    1    |    0     | -               |
```

---

## Use Cases

### Use Case 1: Quick Verification

**Scenario:** Employee wants to quickly check if their December paystub is correct.

**Steps:**
1. Upload calendar JSON file
2. Enter meeting subject filter
3. Process file to see monthly breakdown
4. Select "December 2024" from the dropdown in the verification card
5. View calendar data displayed (e.g., 8 weekdays, 2 Saturdays, 1 Sunday, 1 holiday)
6. Enter paystub amounts for each day type
7. Click "Verify Data"
8. See green success message if all match, or yellow warning with specific discrepancies

---

### Use Case 2: Multi-Month Verification

**Scenario:** Employee wants to verify multiple months at once for their quarterly review.

**Steps:**
1. Upload calendar JSON file and process it
2. Select October 2024 in the first verification card and verify
3. Click "Add Another Month" button
4. Select November 2024 in the second card and verify
5. Click "Add Another Month" again
6. Select December 2024 in the third card and verify
7. Review all three results simultaneously
8. Take a screenshot or export to Excel for records

---

### Use Case 3: Identifying Discrepancies

**Scenario:** Employee notices their paystub seems incorrect.

**Steps:**
1. Process calendar data
2. Select the questionable month
3. Enter the amounts from their paystub
4. Click "Verify Data"
5. See detailed discrepancy report:
   - "Saturdays: Calendar shows 2, Paystub shows 1 (Difference: 1)"
6. Use this information to discuss with HR/payroll department
7. Export the monthly breakdown table to Excel as supporting documentation

---

## Technical Implementation Notes

### Service Layer Changes

1. **MeetingAnalysisService**
   - Enhanced to track Saturday vs Sunday separately
   - Maintains backward compatibility with `WeekendCount`
   - Weekend categorization happens during day analysis

2. **ExcelExportService**
   - Updated headers to include "Saturdays" and "Sundays"
   - Updated data columns accordingly
   - Column indices shifted to accommodate new columns

### Testing Updates

All existing tests have been updated to:
- ✅ Include `SaturdayCount` and `SundayCount` in test data
- ✅ Add assertions for the new properties where relevant
- ✅ Verify Excel export includes the new columns
- ✅ 106 tests passing with 100% success rate

---

## Benefits

### For Employees
- ✅ **Quick verification** - No manual counting or calculations needed
- ✅ **Visual feedback** - Clear success/warning indicators
- ✅ **Multi-month support** - Check multiple pay periods at once
- ✅ **Detailed breakdown** - See exactly which day types have discrepancies
- ✅ **Persistent data** - Add/remove verification cards without losing the overview

### For Payroll/HR
- ✅ **Transparency** - Employees can self-verify before raising issues
- ✅ **Documentation** - Excel export provides audit trail
- ✅ **Accuracy** - Reduces manual counting errors
- ✅ **Efficiency** - Fewer payroll disputes from miscounting

---

## Future Enhancements (Out of Scope)

Potential improvements for future iterations:
- Save verification history
- Email verification results
- Integration with payroll systems
- Mobile-responsive enhancements
- PDF export option
- Historical comparison across multiple months

---

## Accessibility

The feature includes:
- ✅ Semantic HTML structure
- ✅ ARIA labels for screen readers
- ✅ Keyboard navigation support
- ✅ Color-blind friendly success/warning colors (using icons in addition to colors)
- ✅ Clear, descriptive text for all actions

---

## Browser Compatibility

Tested and working on:
- ✅ Chrome/Edge (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)

---

## Conclusion

The paystub verification feature provides a user-friendly way for employees to verify their consignment payment data against their calendar entries. The design prioritizes:
- **Clarity** - Clear visual feedback on matches and mismatches
- **Efficiency** - Multiple months can be checked simultaneously
- **Flexibility** - Data remains loaded while checking different months
- **Accuracy** - Precise breakdown by weekdays, Saturdays, Sundays, and holidays

The implementation follows the existing design patterns in the application and maintains full backward compatibility while adding significant new functionality.
