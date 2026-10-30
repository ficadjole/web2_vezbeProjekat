$api = "http://localhost:8453/api"
$books = 1..20 | ForEach-Object { @{ id = "978-86-521-{0:D4}-0" -f $_; title = "Knjiga $_"; author = "Autor $_"; price = 10 + $_; quantity = 50 } }
foreach ($b in $books) { Invoke-RestMethod -Method Post -Uri "$api/Books" -ContentType "application/json" -Body ($b | ConvertTo-Json) }
foreach ($i in 1..10) {
    $acc = @{ id = "user$i"; owner = "Klijent $i"; balance = 1000 } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri "$api/Accounts" -ContentType "application/json" -Body $acc
}
# klijent bez dovoljno novca – za Vježbu 3
Invoke-RestMethod -Method Post -Uri "$api/Accounts" -ContentType "application/json" `
    -Body (@{ id = "user0"; owner = "Klijent bez novca"; balance = 5 } | ConvertTo-Json)
