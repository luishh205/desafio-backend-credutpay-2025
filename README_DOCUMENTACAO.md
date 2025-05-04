Documenta��o da API - Usu�rio

Endpoint: GET /api/usuario
Descri��o: Retorna uma lista de todos os usu�rios cadastrados no banco de dados.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": true,
"data": [
{
"id": 1,
"email": "usuario1@example.com",
"password": "senha123"
}
],
"message": null
}

Endpoint: GET /api/usuario/{id}
Descri��o: Retorna os detalhes de um usu�rio com base no ID fornecido.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": true,
"data": {
"id": 1,
"email": "usuario1@example.com",
"password": "senha123"
},
"message": null
}

Endpoint: POST /api/usuario/register
Descri��o: Cria um novo usu�rio com os dados fornecidos.
Autentica��o: N�o requer autentica��o.

Corpo da Requisi��o:
{
"email": "novo_usuario@example.com",
"password": "senha123"
}

Modelo de retorno
{
"success": true,
"data": 1, retorna o id do usuario
"message": null
}

Endpoint: POST /api/usuario/login
Descri��o: Realiza o login do usu�rio e retorna um token JWT.
Autentica��o: N�o requer autentica��o.

Corpo da Requisi��o:
{
"email": "usuario@example.com",
"password": "senha123"
}

Modelo de retorno
{
"success": true,
"data": {
"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
},
"message": null
}

Endpoint: PUT /api/usuario
Descri��o: Atualiza os dados de um usu�rio existente.
Autentica��o: Requer token JWT.

Corpo da Requisi��o:
{
"id": 1,
"email": "usuario_atualizado@example.com",
"password": "nova_senha123"
}

Modelo de retorno
{
"success": false,
"data": null,
"message": "Dados inv�lidos fornecidos."
}

Endpoint: DELETE /api/usuario/{id}
Descri��o: Exclui um usu�rio com base no ID fornecido.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": false,
"data": null,
"message": "Usu�rio com ID 1 n�o encontrado."
}

Documenta��o da API - Conta

Endpoint: GET /api/contas
Descri��o: Retorna uma lista de todas as contas cadastradas no banco de dados.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": true,
"data": [
{
"id": 1,
"nome": "Empresa A",
"endereco": "",
"email": "empresaA@example.com",
"telefone": "11999999999",
"cpf": "12345678000100",
"numeroConta": 12345,
"agencia": 6789,
}
],
"message": null
}

Endpoint: GET /api/contas/{id}
Descri��o: Retorna os detalhes de uma conta com base no ID fornecido.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": true,
"data": {
"id": 1,
"nome": "Empresa A",
"endero": "",
"email": "empresaA@example.com",
"telefone": "11999999999",
"cpf": "12345678000100",
"numeroConta": 12345,
"agencia": 6789,
},
"message": null
}

Endpoint: POST /api/contas
Descri��o: Cria uma nova conta com os dados fornecidos.
Autentica��o: Requer token JWT.

Corpo da Requisi��o:
Formato: multipart/form-data
Campos:
cpf (string, obrigat�rio): cpf .
numeroConta (int, obrigat�rio): N�mero da conta.
agencia (int, obrigat�rio): Ag�ncia da conta.
endereco (string, obrigat�rio)
nome (string, obrigat�rio)
email (string, obrigat�rio)

Modelo de retorno
{
"success": true,
"data": 1, // ID da conta criada
"message": null
}

Endpoint: PUT /api/contas
Descri��o: Atualiza os dados de uma conta existente.
Autentica��o: Requer token JWT.

Corpo da Requisi��o:
Formato: multipart/form-data
Campos:
id (int, obrigat�rio): ID da conta.
cpf (string, obrigat�rio):
numeroConta (int, obrigat�rio): N�mero da conta.
agencia (int, obrigat�rio): Ag�ncia da conta.

Modelo de retorno
{
"success": true,
"data": 1, // ID da conta atualizada
"message": null
}

Endpoint: DELETE /api/contas/{id}
Descri��o: Exclui uma conta com base no ID fornecido.
Autentica��o: Requer token JWT.

Par�metros:
id (int, obrigat�rio): ID da conta.

Modelo de retorno
{
"success": false,
"data": null,
"message": "Conta com ID 1 n�o encontrada."
}

Documenta��o da API - Transa��o

Endpoint: GET /api/transacao/saldo/{id}
Descri��o: Retorna o saldo atual de uma conta com base no ID fornecido.
Autentica��o: Requer token JWT.

Modelo de retorno
{
"success": true,
"message": null,
"data": [
{
"id": 0,
"valor": 0,
"saldo": 500000.60,
"tipo": 0,
"tipoNome": "Saldo",
"contaId": 22,
"contaDestinoId": 0,
"dataTransacao": "0001-01-01T00:00:00",
"conta": {
"id": 22,
"cpf": "19368947000188",
"numeroConta": 1012,
"agencia": 1020,
"nome": "AM COMERCIO DE MATERIAIS LTDA",
"email": "amcomercio@globomail.com",
"telefone": "(21) 3254-2017 / (21) 3254-2027"
},
"contaDestino": null
}
]
}

Endpoint: GET /api/transacao/extrato/{id}
Descri��o: Retorna o extrato de transa��es de uma conta com base no ID fornecido, com filtro de período opcional.
Autentica��o: Requer token JWT.

Par�metros:
id (int, obrigat�rio): ID da conta.
dataInicial(dateTime):2025-05-01 21:37:47
dataFinal(dateTime):2025-05-01 21:38:53

Modelo de retorno
{
"success": true,
"message": null,
"data": [
{
"id": 96,
"valor": 500000.00,
"saldo": 0,
"tipo": 2,
"tipoNome": "Deposito",
"contaId": 22,
"contaDestinoId": 0,
"dataTransacao": "2025-03-15T22:39:08-03:00",
"conta": {
"id": 22,
"cpf": "19368947000188",
"numeroConta": 1012,
"agencia": 1020,
"nome": "AM COMERCIO DE MATERIAIS LTDA",
"endereco": "AM PAPELARIA",
"email": "amcomercio@globomail.com",
"telefone": "(21) 3254-2017 / (21) 3254-2027"
},
"contaDestino": {
"id": 0,
"cpf": "00.000.000/0001-00",
"numeroConta": 0,
"agencia": 0,
"nome": null,
"endereco": null,
"email": null,
"telefone": null
}
}
]
}

Endpoint: POST /api/transacao/deposito
Descri��o: Realiza um dep�sito em uma conta.
Autentica��o: Requer token JWT.

Par�metros:
ContaId (int, obrigat�rio): ID da conta.
Valor (decimal, obrigat�rio): Valor a ser transferido.

Modelo de retorno
{
"success": true,
"data": true,
"message": null
}

Endpoint: POST /api/transacao/saque
Descri��o: Realiza um saque em uma conta.
Autentica��o: Requer token JWT.

Par�metros:
ContaId (int, obrigat�rio): ID da conta que realizar� o saque.
Valor (decimal, obrigat�rio): Valor a ser sacado.

Modelo de retorno
{
"success": true,
"data": true,
"message": null
}

Endpoint: POST /api/transacao/transferencia
Descri��o: Realiza uma transfer�ncia entre duas contas.
Autentica��o: Requer token JWT.

Par�metros:
contaOrigemId (int, obrigat�rio): ID da conta de origem.
contaDestinoId (int, obrigat�rio): ID da conta de destino.
Valor (decimal, obrigat�rio): Valor a ser transferido.

Modelo de retorno
{
"success": true,
"data": true,
"message": null
}
