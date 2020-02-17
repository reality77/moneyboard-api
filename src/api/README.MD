# moneyboard api

## Import 

### Creating account
curl --insecure https://localhost:5001/account/debugadd?name=test

### Prepare import
curl --insecure -X POST https://localhost:5001/import/prepare?accountId=4 -F file=@test.qif > liste_import.json

### Upload to account

Extraire un élément dans liste_import.json et créer un fichier import.json

curl --insecure -X POST https://localhost:5001/import/uploadtoaccount -H "Content-Type:application/json;charset=utf-8" -d @import.json