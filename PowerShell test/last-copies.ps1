$api    = "http://localhost:8453/api"
$bookId = "978-86-521-0002-0"
$ids    = 1..50 | ForEach-Object { [guid]::NewGuid() }
$sumBefore = (1..10 | ForEach-Object { (Invoke-RestMethod "$api/Accounts/user$_").balance } | Measure-Object -Sum).Sum

$jobs = foreach ($i in 0..49) {
    Start-Job -ArgumentList $api, $bookId, $i, $ids[$i] -ScriptBlock {
        param($api, $bookId, $i, $id)
        $body = @{ userId = "user$($i % 10 + 1)"; bookId = $bookId } | ConvertTo-Json
        Invoke-RestMethod -Method Post -Uri "$api/Purchases" -ContentType "application/json" `
            -Headers @{ "Idempotency-Key" = $id } -Body $body | Out-Null
    }
}
$jobs | Wait-Job | Out-Null
Start-Sleep -Seconds 20       # sage se završavaju asinhrono

$ids | ForEach-Object { (Invoke-RestMethod "$api/Purchases/$_").status } | Group-Object | Format-Table Name, Count
$book     = Invoke-RestMethod "$api/Books/$bookId"
$sumAfter = (1..10 | ForEach-Object { (Invoke-RestMethod "$api/Accounts/user$_").balance } | Measure-Object -Sum).Sum
"Zaliha: $($book.quantity)   rezervisano: $($book.reserved)   potrošeno: $($sumBefore - $sumAfter)   očekivano: $(10 * $book.price)"
Read-Host
