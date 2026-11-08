Connect-ServiceFabricCluster -ConnectionEndpoint localhost:19000 | Out-Null
$api     = "http://localhost:8453/api"
$bookId  = "978-86-521-0001-0"
$service = "fabric:/ProjekatVezbeWeb/LibraryStatefulService"
$ok = 0

foreach ($i in 1..200) {
    if ($i -in 50, 120) {
        # premjesti primarne replike svih particija knjižare usred opterećenja
        Get-ServiceFabricPartition -ServiceName $service | ForEach-Object {
            Move-ServiceFabricPrimaryReplica -PartitionId $_.PartitionId -ServiceName $service | Out-Null
        }
    }
    try {
        Invoke-RestMethod -Method Post -Uri "$api/Books/$bookId/reservations" `
            -ContentType "application/json" -Body '{ "quantity": 1 }' -TimeoutSec 5 | Out-Null
        $ok++
    } catch { Write-Warning "Zahtjev $i nije uspio: $($_.Exception.Message)" }
}
"Uspješnih odgovora: $ok"
Invoke-RestMethod "$api/Books/$bookId/consistency"
