# Test Cases Documentation

## File Excel

**File**: `TestCases_AllModules.csv`

### Format
- **Encoding**: UTF-8
- **Delimiter**: Comma (,)
- **Quote Character**: Double quote (")

### Columns
1. **STT** - Serial number
2. **Test Case ID** - Unique test case identifier (TC-XXX-XXX)
3. **Test Case Name** - Test case name in English
4. **Test Scenario** - Brief scenario description in English
5. **Precondition** - Prerequisites in English
6. **Test Steps** - Step-by-step instructions in English
7. **Expected Result** - Expected outcome in English
8. **Priority** - High/Medium/Low
9. **Status** - ✅ Implemented / ⏳ Not Implemented
10. **Module** - Module name

### Language
- **Descriptions**: English (basic)
- **Input examples**: Vietnamese (if needed for clarity)
- **API endpoints**: English
- **Status values**: English

## How to Open

### Option 1: Excel
1. Open Excel
2. Data > Get Data > From File > From Text/CSV
3. Select `TestCases_AllModules.csv`
4. Choose encoding: **UTF-8**
5. Click "Load"

### Option 2: Google Sheets
1. Open Google Sheets
2. File > Import > Upload
3. Select `TestCases_AllModules.csv`
4. Choose "Replace spreadsheet"
5. Import location: "Replace current sheet"

### Option 3: Direct Open (Windows)
- Right-click file > Open with > Excel
- Excel will auto-detect UTF-8

## Statistics

- **Total Test Cases**: 94
- **Implemented**: 60+
- **Not Implemented**: 30+
- **Modules**: 20

## Notes

- All test descriptions use basic English
- Vietnamese is only used for input examples when necessary
- CSV format follows RFC 4180 standard
- Quotes are properly escaped (double quotes for quotes inside values)














