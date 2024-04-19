# PowerShell script to parse Checkstyle XML and find severity "error"
# This script exits with an exception if any errors are found, otherwise it exits with code 0 to make the pipeline fail.
$ScriptDir = $PSScriptRoot
Write-Host "PS: Checking for severity=Error in VIAresults.xml, current script directory is $ScriptDir"
# from the $ScriptDir, go one folder up and then into the "Artefact" folder
$relativePath = "$ScriptDir\..\Artefacts\VIAresults.xml"
$checkstyleReportPath = Resolve-Path $relativePath
# Output directory to console with prefix "PS: "
Write-Host "PS: Checkstyle report path is $checkstyleReportPath"
# Check if the file exists
if (-not (Test-Path -Path $checkstyleReportPath)) {
    Write-Error "Checkstyle report not found at $checkstyleReportPath"
    exit 1
}

# Load the XML file
[xml]$checkstyleXml = Get-Content $checkstyleReportPath

# Find all errors with severity "error"
$errors = $checkstyleXml.checkstyle.file.error | Where-Object { $_.severity -eq "error" }

# Check if there are any errors
if ($errors.Count -gt 0) {
    $errorMessage = "Errors found in Checkstyle report:`n"
    foreach ($error in $errors) {
        $errorMessage += "File: $($error.ParentNode.name), Message: $($error.message), Source: $($error.source)`n"
    }
    throw $errorMessage
}
else {
    Write-Host "No errors of severity 'error' found in Checkstyle report."
}
