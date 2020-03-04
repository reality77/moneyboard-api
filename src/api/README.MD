# moneyboard api

## Import 

### Creating account

```bash
curl --insecure https://localhost:5001/account/debugadd?name=test
```

### Import

```bash
curl --insecure -X POST https://localhost:5001/import?accountId=1 -F file=@sample/sample1.qif
```

### Get transactions by tag

```bash
curl --insecure -X GET 'https://localhost:5001/transactions/tag/category/factures?searchSubTags=true'
```

### Liste des tags

```bash
curl --insecure -X GET https://localhost:5001/tags/
```

### Merge tags

Lors de la phase de détection des tags (payee principalement), il est possible que plusieurs clés détectées correspondent
au même tag dans la réalité.
Cette méthode permet de fusionner deux tags (payees ou autre), et de créer une règle d'import TagRecognition qui 
permet de faire en sorte que les futurs imports soient directement liés à la clé destination.

Exemple:
Lors d'un import, détection auto de 2 tags `payee/m_dupont` et `payee/mr_dupont` :
Avec le merge, on va faire en sorte que 
- les transactions tagguées sur `payee/m_dupont` soient retagguées sur `payee/mr_dupont`
- `payee/m_dupont` disparaisse (au profit de `payee/mr_dupont`)
- `payee/m_dupont` ne soit pas recréé dans les imports à venir et que ceux ci utilisent `payee/mr_dupont`

```bash
curl --insecure -X POST https://localhost:5001/tags/<type>/<source>/merge?target=<target>
```

