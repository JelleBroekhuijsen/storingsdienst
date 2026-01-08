# Power Automate Flow Guide: Export Calendar Meetings to JSON

## Overview

For organizations that cannot add Azure AD app registrations, you can create a Power Automate flow to export calendar data to JSON format. This guide will walk you through creating the flow step-by-step.

## Prerequisites

- Microsoft 365 account with Power Automate access
- OneDrive for Business or SharePoint access

## Step-by-Step Instructions

### Step 1: Create New Power Automate Flow

1. Go to [https://make.powerautomate.com](https://make.powerautomate.com)
2. Click **"Create"** → **"Instant cloud flow"**
3. **Name**: "Export Calendar Meetings to JSON"
4. **Trigger**: Select "Manually trigger a flow"
5. Click **"Create"**

### Step 2: Add Input Parameters

1. In the manual trigger, click **"Add an input"**
2. Select **"Text"**
3. **Input name**: "Subject Filter"
4. Mark as **Required**

![Step 2: Get calendar view of events](https://github.com/user-attachments/assets/bc64c5d6-4522-4f1f-962e-e9a4208e2d55)

### Step 3: List Calendar Events

1. Click **"+ New step"**
2. Search for and select **"Office 365 Outlook"**
3. Choose action: **"Get calendar view of events (V3)"**
4. Configure parameters:
   - **Calendar id**: `Calendar`
   - **Start Time**: `addDays(utcNow(), -365)`
   - **End Time**: `utcNow()`
   - **Time Zone**: `UTC`
   - **Top Count**: `1000`

![Step 3: Filter array](https://github.com/user-attachments/assets/e50562af-8b5c-4aaa-8b9b-e9579cd79202)

### Step 4: Filter Events by Subject

1. Click **"+ New step"**
2. Search for and select **"Data Operation"**
3. Choose **"Filter array"**
4. **From**: Click in the field and select "value" from the dynamic content (from previous step)
5. Configure condition:
   - Click **"Edit in advanced mode"**
   - Paste: `@contains(item()?['subject'], triggerBody()['text'])`

![Step 4: Select](https://github.com/user-attachments/assets/a3c1ff9d-4af0-494d-bc9d-08be9d9e9785)

### Step 5: Transform to Required JSON Format

1. Click **"+ New step"**
2. Select **"Data Operation"** → **"Select"**
3. **From**: Output from Filter array action
4. Click **"Map"** and add these fields:
   - **subject**: `item()?['subject']`
   - **start**: Click "Switch to input entire array" and enter:
     ```
     {
       "dateTime": "@{item()?['start']?['dateTime']}",
       "timeZone": "@{item()?['start']?['timeZone']}"
     }
     ```
   - **end**: Click "Switch to input entire array" and enter:
     ```
     {
       "dateTime": "@{item()?['end']?['dateTime']}",
       "timeZone": "@{item()?['end']?['timeZone']}"
     }
     ```
   - **isAllDay**: `item()?['isAllDay']`

![Step 5: Compose](https://github.com/user-attachments/assets/6494b0ba-3b8e-4e66-a309-31fd72b37d4c)

### Step 6: Create JSON Wrapper

1. Click **"+ New step"**
2. Select **"Data Operation"** → **"Compose"**
3. In the **Inputs** field, switch to code view and paste:
   ```json
   {
     "events": @{body('Select')},
     "exportDate": "@{utcNow()}",
     "subjectFilter": "@{triggerBody()['text']}"
   }
   ```

### Step 7: Save to OneDrive

1. Click **"+ New step"**
2. Search for and select **"OneDrive for Business"**
3. Choose **"Create file"**
4. Configure:
   - **Folder Path**: `/Documents/CalendarExports` (create this folder in OneDrive first)
   - **File Name**: `calendar_export_@{formatDateTime(utcNow(), 'yyyyMMdd_HHmmss')}.json`
   - **File Content**: Select "Outputs" from the Compose step

![Step 7: Create file](https://github.com/user-attachments/assets/a56dfefa-41ec-422e-a415-762145b0b0e3)

### Step 8 (Optional): Send Email Notification

1. Click **"+ New step"**
2. Select **"Office 365 Outlook"** → **"Send an email (V2)"**
3. Configure:
   - **To**: Your email address
   - **Subject**: `Calendar Export Complete`
   - **Body**: `Your calendar export is ready in OneDrive: /Documents/CalendarExports`

## Alternative: Save to SharePoint

Instead of OneDrive (Step 7), you can use SharePoint:

1. Click **"+ New step"**
2. Select **"SharePoint"** → **"Create file"**
3. Configure:
   - **Site Address**: Select your SharePoint site
   - **Folder Path**: `/Shared Documents/CalendarExports`
   - **File Name**: `calendar_export_@{formatDateTime(utcNow(), 'yyyyMMdd_HHmmss')}.json`
   - **File Content**: Select "Outputs" from the Compose step

## Using the Flow

1. Go to [https://make.powerautomate.com](https://make.powerautomate.com)
2. Click on **"My flows"**
3. Find your flow and click **"Run"**
4. Enter the meeting subject you want to filter (e.g., "Project Standup")
5. Click **"Run flow"**
6. Go to OneDrive → Documents → CalendarExports
7. Download the latest JSON file
8. Upload it to the Storingsdienst web application

## Expected JSON Output Format

Your flow will generate a JSON file with this structure:

```json
{
  "events": [
    {
      "subject": "Project Standup",
      "start": {
        "dateTime": "2024-01-15T09:00:00",
        "timeZone": "UTC"
      },
      "end": {
        "dateTime": "2024-01-15T09:30:00",
        "timeZone": "UTC"
      },
      "isAllDay": false
    }
  ],
  "exportDate": "2025-01-15T14:30:00Z",
  "subjectFilter": "Project Standup"
}
```

## Troubleshooting

### Flow fails with "Unauthorized"
- Ensure you've authorized the Office 365 Outlook and OneDrive connections
- Go to Connections and re-authenticate if needed

### No events in the output
- Check that your subject filter matches actual meeting titles (case-sensitive!)
- Verify meetings exist in the past year
- Try a partial match (e.g., "Standup" instead of "Project Standup Meeting")

### JSON file not found in OneDrive
- Ensure the `/Documents/CalendarExports` folder exists
- Check the flow run history for errors
- Verify OneDrive connection is authorized

### Invalid JSON error in the app
- Download and open the JSON file in a text editor
- Verify it matches the expected format above
- Check for any error messages in the file

## Security & Privacy

- The flow runs entirely within your Microsoft 365 tenant
- No data is sent to external services
- JSON files are stored in your personal OneDrive
- You can delete JSON files after importing them to the app

## Support

If you encounter issues:
1. Check the flow run history in Power Automate
2. Review error messages in each step
3. Verify all connections are authorized
4. Ensure you have the necessary Microsoft 365 licenses
