﻿using Dapper;
using desafio_backend_2025.Models;
using Microsoft.AspNetCore.Mvc;

namespace desafio_backend_2025.Repositories
{
    public class TransacaoRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ContaRepository _ContaRepository;
        public TransacaoRepository(DatabaseConnection db,ContaRepository contaRepository)
        {
            _ContaRepository = contaRepository;
            _db = db;
        }

        internal async Task<Response<IEnumerable<Transacao>>> GetExtratoContaId(int id, DateTime? dataInicial = null, DateTime? dataFinal = null)
        {
            try
            {
                if (id <= 0)
                {
                    return Response<IEnumerable<Transacao>>.Error("ID deve ser maior que zero.");
                }

                var contaResponse = await _ContaRepository.GetById(id);
                if (!contaResponse.Success || contaResponse.Data == null)
                {
                    return Response<IEnumerable<Transacao>>.Error($"Conta com ID {id} não encontrada.");
                }

                //using var conn = _db.GetConnection();

                //var extrato = await conn.QueryAsync<Transacao>(
                //"SELECT * FROM transacoes WHERE contaId = @Id ORDER BY dataTransacao DESC",
                //new { Id = id });

                using var conn = _db.GetConnection();

                var sql = @"SELECT * FROM transacoes 
                    WHERE contaId = @Id";

                if (dataInicial.HasValue)
                    sql += " AND dataTransacao >= @DataInicial";

                if (dataFinal.HasValue)
                    sql += " AND dataTransacao <= @DataFinal";

                sql += " ORDER BY dataTransacao DESC";

                var extrato = await conn.QueryAsync<Transacao>(
                    sql,
                    new { Id = id, DataInicial = dataInicial, DataFinal = dataFinal });

                foreach (var transacao in extrato)
                {
                    transacao.Conta = await conn.QueryFirstOrDefaultAsync<Conta>(
                        "SELECT * FROM Conta WHERE id = @ContaId",
                        new { ContaId = transacao.ContaId });

                    if (transacao.Conta == null)
                    {
                        transacao.Conta = new Conta { CPF = "000.000.000.00" };
                    }

                    transacao.ContaDestino = await conn.QueryFirstOrDefaultAsync<Conta>(
                        "SELECT * FROM Conta WHERE id = @ContaId",
                        new { ContaId = transacao.ContaDestinoId });

                    if (transacao.ContaDestino == null)
                    {
                        transacao.ContaDestino = new Conta { CPF = "000.000.000.00" };
                    }
                }

                if (extrato == null || !extrato.Any())
                {
                    return Response<IEnumerable<Transacao>>.Ok(new List<Transacao>());
                }

                return Response<IEnumerable<Transacao>>.Ok(extrato);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Transacao>>.Error($"Erro ao buscar extrato da conta com ID {id}: {ex.Message}");
            }
        }
       
        public async Task<Response<IEnumerable<Transacao>>> GetSaldo(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Response<IEnumerable<Transacao>>.Error("ID deve ser maior que zero.");
                }

                var contaResponse = await _ContaRepository.GetById(id);
                if (!contaResponse.Success || contaResponse.Data == null)
                {
                    return Response<IEnumerable<Transacao>>.Error($"Conta com ID {id} não encontrada.");
                }

                using var conn = _db.GetConnection();

                var saldoConta = await conn.QueryAsync<Transacao>(
                @"SELECT 
                    SUM(CASE 
                            WHEN tipo = 'deposito' THEN valor
                            WHEN tipo = 'saque' THEN -valor
                            WHEN tipo = 'transferencia' THEN -valor
                            ELSE 0
                        END) AS saldo,
                        contaId
                FROM crud.transacoes
                WHERE contaId = @Id ",
                new { Id = id });
                
                foreach (var transacao in saldoConta)
                {
                    transacao.Conta = await conn.QueryFirstOrDefaultAsync<Conta>(
                        "SELECT * FROM Conta WHERE id = @ContaId",
                        new { ContaId = transacao.ContaId });
                }

                if (saldoConta == null || !saldoConta.Any())
                {
                    return Response<IEnumerable<Transacao>>.Ok(new List<Transacao>());
                }

                return Response<IEnumerable<Transacao>>.Ok(saldoConta);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Transacao>>.Error($"Erro ao buscar extrato da conta com ID {id}: {ex.Message}");
            }
        }

        internal async Task<Response<bool>> Depositar(int contaId, decimal valor)
        {
            try
            {
                if (valor <= 0)
                    return Response<bool>.Error("O valor do depósito deve ser maior que zero.");

                using var conn = _db.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // Bloqueia a conta para evitar concorrência
                    var conta = await conn.QueryFirstOrDefaultAsync<Conta>(
                        "SELECT * FROM Conta WHERE id = @Id FOR UPDATE",
                        new { Id = contaId },
                        transaction);

                    if (conta == null)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error($"Conta com ID {contaId} não encontrada.");
                    }

                    var transacao = new Transacao
                    {
                        ContaId = contaId,
                        Valor = valor,
                        DataTransacao = DateTime.Now
                    };

                    string sql = @"INSERT INTO transacoes (contaId, valor, tipo, dataTransacao) 
                           VALUES (@ContaId, @Valor, 'deposito', @DataTransacao);";

                    int retorno = await conn.ExecuteAsync(sql, transacao, transaction);

                    if (retorno == 0)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error("Falha ao inserir a transação no banco de dados.");
                    }

                    transaction.Commit();
                    return Response<bool>.Ok(true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Response<bool>.Error($"Erro ao realizar depósito: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Response<bool>.Error($"Erro ao realizar depósito: {ex.Message}");
            }
        }

        public async Task<Response<decimal>> VerificaSaldoConta(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Response<decimal>.Error("ID deve ser maior que zero.");
                }

                using var conn = _db.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    string sql = @"
                                SELECT 
                                    SUM(CASE 
                                            WHEN tipo = 'deposito' THEN valor
                                            WHEN tipo = 'saque' THEN -valor
                                            WHEN tipo = 'transferencia' THEN -valor
                                            ELSE 0
                                        END) AS saldo
                                FROM transacoes
                                WHERE contaId = @Id;";

                    var saldo = await conn.QueryFirstOrDefaultAsync<decimal?>(sql, new { Id = id }, transaction);

                    transaction.Commit();
                    return Response<decimal>.Ok(saldo ?? 0);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Response<decimal>.Error($"Erro ao buscar saldo da conta com ID {id}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Response<decimal>.Error($"Erro ao buscar saldo da conta com ID {id}: {ex.Message}");
            }
        }

        internal async Task<Response<bool>> Sacar(int contaId, decimal valor)
        {
            try
            {
                if (valor <= 0)
                    return Response<bool>.Error("O valor do saque deve ser maior que zero.");

                using var conn = _db.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    //Bloqueia a conta para evitar concorrência
                    var conta = await conn.QueryFirstOrDefaultAsync<Conta>(
                        "SELECT * FROM Conta WHERE id = @Id FOR UPDATE",
                        new { Id = contaId },
                        transaction);

                    if (conta == null)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error($"Conta com ID {contaId} não encontrada.");
                    }

                    var saldoResponse = await VerificaSaldoConta(contaId);
                    if (!saldoResponse.Success)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error(saldoResponse.Message);
                    }

                    if (saldoResponse.Data < valor)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error("Saldo insuficiente para saque.");
                    }

                    var transacao = new Transacao
                    {
                        ContaId = contaId,
                        Valor = valor,
                        DataTransacao = DateTime.Now
                    };

                    string sql = @"INSERT INTO transacoes (ContaId, Valor, Tipo, DataTransacao) 
                           VALUES (@ContaId, @Valor, 'saque', @DataTransacao);";

                    int retorno = await conn.ExecuteAsync(sql, transacao, transaction);

                    if (retorno == 0)
                    {
                        transaction.Rollback();
                        return Response<bool>.Error("Falha ao inserir a transação no banco de dados.");
                    }

                    transaction.Commit();
                    return Response<bool>.Ok(true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Response<bool>.Error($"Erro ao realizar saque: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Response<bool>.Error($"Erro ao realizar saque: {ex.Message}");
            }
        }

        internal async Task<Response<bool>> Transferir(int contaOrigemId, int contaDestinoId, decimal valor)
        {
            try
            {
                if (valor <= 0)
                    return Response<bool>.Error("O valor da transferência deve ser maior que zero.");
                if (contaOrigemId == contaDestinoId)
                    return Response<bool>.Error("Não é possível transferir para a mesma conta.");

                // Verificar se as contas existem
                var contaOrigem = await _ContaRepository.GetById(contaOrigemId);
                var contaDestino = await _ContaRepository.GetById(contaDestinoId);

                if (contaOrigem == null || contaDestino == null)
                {
                    return Response<bool>.Error("Uma ou ambas as contas não existem.");
                }


                var saldoResponse = await VerificaSaldoConta(contaOrigemId);
                if (!saldoResponse.Success)
                    return Response<bool>.Error(saldoResponse.Message);

                if (saldoResponse.Data < valor)
                    return Response<bool>.Error("Saldo insuficiente para transferência.");

                using var conn = _db.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    string sqlOrigem = @"INSERT INTO transacoes (ContaId, Valor, Tipo, DataTransacao, ContaDestinoId) 
                           VALUES (@ContaId, @Valor, 'transferencia', @DataTransacao, @ContaDestinoId);";

                    var transacaoOrigem = new Transacao
                    {
                        ContaId = contaOrigemId,
                        Valor = valor,
                        DataTransacao = DateTime.Now,
                        ContaDestinoId = contaDestinoId
                    };
                    int TransacaoOrigem = await conn.ExecuteAsync(sqlOrigem, transacaoOrigem, transaction);
                    if (TransacaoOrigem == 0)
                    {
                        return Response<bool>.Error($"Falha ao inserir a transação no banco de dados. (TransacaoOrigem)");
                    }

                    string sqlDestino = @"INSERT INTO transacoes (ContaId, Valor, Tipo, DataTransacao) 
                           VALUES (@ContaId, @Valor, 'deposito', @DataTransacao);";

                    var transacaoDestino = new Transacao
                    {
                        ContaId = contaDestinoId,
                        Valor = valor,
                        DataTransacao = DateTime.Now
                    };

                    int TransacaoDestino = await conn.ExecuteAsync(sqlDestino, transacaoDestino, transaction);
                    if (TransacaoDestino == 0)
                    {
                        return Response<bool>.Error($"Falha ao inserir a transação no banco de dados. (TransacaoDestino)");
                    }

                    transaction.Commit();

                    return Response<bool>.Ok(true);
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    return Response<bool>.Error($"Falha ao inserir a transação no banco de dados. {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Response<bool>.Error($"Erro ao realizar transferência: {ex.Message}");
            }
        }

    }
}
