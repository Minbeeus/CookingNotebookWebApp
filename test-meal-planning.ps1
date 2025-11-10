# PowerShell Script để test Meal Planning API
# Sử dụng: powershell -ExecutionPolicy Bypass -File test-meal-planning.ps1

$baseUrl = "http://localhost:5000"
$contentType = "application/json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🧪 Meal Planning API Test Suite" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Hàm để format JSON response
function Format-JsonOutput {
    param([string]$json)
    try {
        $obj = ConvertFrom-Json $json
        return $obj | ConvertTo-Json -Depth 10
    }
    catch {
        return $json
    }
}

# Test 1: Health Check
Write-Host "📋 Test 1: Health Check" -ForegroundColor Yellow
Write-Host "URL: GET $baseUrl/api/mealplanning/health" -ForegroundColor Gray
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/health" -Method Get
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ($response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10) -ForegroundColor White
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Get MealTimes
Write-Host "📋 Test 2: Get MealTimes" -ForegroundColor Yellow
Write-Host "URL: GET $baseUrl/api/mealplanning/mealtimes" -ForegroundColor Gray
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/mealtimes" -Method Get
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $content = $response.Content | ConvertFrom-Json
    Write-Host "Found $($content.mealTimes.Count) meal times:" -ForegroundColor Green
    foreach ($meal in $content.mealTimes) {
        Write-Host "  - [$($meal.mealTimeId)] $($meal.name)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Basic Meal Plan (7 days, 3 meals, 2 people)
Write-Host "📋 Test 3: Basic Meal Plan (7 days, 3 meals, 2 people)" -ForegroundColor Yellow
$payload = @{
    userId = 1
    numDays = 7
    numPeople = 2
    mealTimeIds = @(1, 2, 3)
    restrictions = @()
} | ConvertTo-Json

Write-Host "Payload: $payload" -ForegroundColor Gray
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/generate" `
        -Method Post `
        -Headers @{ "Content-Type" = $contentType } `
        -Body $payload
    
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $content = $response.Content | ConvertFrom-Json
    
    Write-Host "Success: $($content.success)" -ForegroundColor Green
    Write-Host "Message: $($content.message)" -ForegroundColor Green
    Write-Host "Meal Plan Items: $($content.mealPlan.Count)" -ForegroundColor Green
    Write-Host "Shopping List Items: $($content.shoppingList.Count)" -ForegroundColor Green
    
    if ($content.mealPlan) {
        Write-Host ""
        Write-Host "📅 First 3 Meal Plans:" -ForegroundColor Cyan
        $content.mealPlan | Select-Object -First 3 | ForEach-Object {
            Write-Host "  Day $($_.day): $($_.mealName) - $($_.recipeTitle) (Prep: $($_.prepTime)m, Cook: $($_.cookTime)m)" -ForegroundColor White
        }
    }
    
    if ($content.shoppingList) {
        Write-Host ""
        Write-Host "🛒 First 5 Shopping Items:" -ForegroundColor Cyan
        $content.shoppingList | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - $($_.ingredientName): $($_.totalQuantity) $($_.unit)" -ForegroundColor White
        }
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response.Content) {
        Write-Host "Response: $($_.Exception.Response.Content)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 4: Short Plan (3 days, 2 meals, 4 people)
Write-Host "📋 Test 4: Short Plan (3 days, 2 meals, 4 people)" -ForegroundColor Yellow
$payload = @{
    userId = 1
    numDays = 3
    numPeople = 4
    mealTimeIds = @(1, 2)
    restrictions = @()
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/generate" `
        -Method Post `
        -Headers @{ "Content-Type" = $contentType } `
        -Body $payload
    
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $content = $response.Content | ConvertFrom-Json
    
    Write-Host "Success: $($content.success)" -ForegroundColor Green
    Write-Host "Meal Plan Items: $($content.mealPlan.Count) (Expected: 6 = 3 days × 2 meals)" -ForegroundColor Green
    Write-Host "Shopping List Items: $($content.shoppingList.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Validation Error (NumDays = 0)
Write-Host "📋 Test 5: Validation Error (NumDays = 0)" -ForegroundColor Yellow
$payload = @{
    userId = 1
    numDays = 0
    numPeople = 2
    mealTimeIds = @(1, 2, 3)
    restrictions = @()
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/generate" `
        -Method Post `
        -Headers @{ "Content-Type" = $contentType } `
        -Body $payload
    
    Write-Host "⚠️ Status: $($response.StatusCode)" -ForegroundColor Yellow
    $content = $response.Content | ConvertFrom-Json
    Write-Host "Success: $($content.success)" -ForegroundColor Yellow
    Write-Host "Message: $($content.message)" -ForegroundColor Yellow
} catch {
    Write-Host "✅ Expected Error: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response.StatusCode -eq 400) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $content = $responseBody | ConvertFrom-Json
        Write-Host "Error Message: $($content.message)" -ForegroundColor Green
    }
}
Write-Host ""

# Test 6: Validation Error (Empty MealTimeIds)
Write-Host "📋 Test 6: Validation Error (Empty MealTimeIds)" -ForegroundColor Yellow
$payload = @{
    userId = 1
    numDays = 7
    numPeople = 2
    mealTimeIds = @()
    restrictions = @()
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/generate" `
        -Method Post `
        -Headers @{ "Content-Type" = $contentType } `
        -Body $payload
    
    Write-Host "⚠️ Status: $($response.StatusCode)" -ForegroundColor Yellow
} catch {
    Write-Host "✅ Expected Error: $($_.Exception.Message)" -ForegroundColor Green
}
Write-Host ""

# Test 7: With Restrictions
Write-Host "📋 Test 7: Plan with Restrictions" -ForegroundColor Yellow
$payload = @{
    userId = 1
    numDays = 3
    numPeople = 2
    mealTimeIds = @(1, 2, 3)
    restrictions = @("Chay")
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/mealplanning/generate" `
        -Method Post `
        -Headers @{ "Content-Type" = $contentType } `
        -Body $payload
    
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    $content = $response.Content | ConvertFrom-Json
    
    Write-Host "Success: $($content.success)" -ForegroundColor Green
    Write-Host "Meal Plan Items: $($content.mealPlan.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Test Suite Completed" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Check the results above for any failures" -ForegroundColor Gray
Write-Host "2. If errors occur, check:" -ForegroundColor Gray
Write-Host "   - Database connection & data" -ForegroundColor Gray
Write-Host "   - API controller implementation" -ForegroundColor Gray
Write-Host "   - Console logs for detailed errors" -ForegroundColor Gray
Write-Host "3. Open browser to http://localhost:5000/MealPlanning to test UI" -ForegroundColor Gray
Write-Host ""
