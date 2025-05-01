using desafio_backend_2025.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace desafio_backend_2025.Repositories
{
    public class ContaRepository
    {
        private readonly DatabaseConnection _db;

        public ContaRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<Response<IEnumerable<Conta>>> GetAll()
        {
            try
            {
                using var conn = _db.GetConnection();
                var contas = await conn.QueryAsync<Conta>("SELECT * FROM Conta");

                return Response<IEnumerable<Conta>>.Ok(contas);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Conta>>.Error($"Erro ao buscar todas as contas: {ex.Message}");
            }
        }

        public async Task<Response<Conta>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Response<Conta>.Error("ID deve ser maior que zero.");
                }

                using var conn = _db.GetConnection();

                var conta = await conn.QuerySingleOrDefaultAsync<Conta>(
                    "SELECT * FROM Conta WHERE id = @Id",
                    new { Id = id })
                    .ConfigureAwait(false);

                if (conta == null)
                {
                    return Response<Conta>.Error($"Conta com ID {id} não encontrada.");
                }

                return Response<Conta>.Ok(conta);
            }
            catch (Exception ex)
            {
                return Response<Conta>.Error($"Erro ao buscar conta pelo ID: {id} - {ex.Message}");
            }
        }

        public async Task<Response<int>> Create(Conta conta)
        {
            try
            {
                if (!ValidarConta(conta.NumeroConta, conta.Agencia, out string erro))
                {
                    return Response<int>.Error(erro);
                }

                bool valido = ValidarCpf(conta.CPF);
                if (!valido)
                {
                    return Response<int>.Error("Cpf invalido.");
                }

                using var conn = _db.GetConnection(); 

                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    var result = await conn.ExecuteAsync(
                        "INSERT INTO Conta (nome, email, telefone, cpf, numeroConta, agencia, endereco) " +
                        "VALUES (@Nome, @email, @telefone, @CPF, @NumeroConta, @Agencia, @endereco)",
                        conta, transaction);

                    transaction.Commit();

                    return Response<int>.Ok(result);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Response<int>.Error($"Erro ao criar conta com : {conta.CPF} - {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Response<int>.Error($"Erro ao criar conta com CPF: {conta.CPF} - {ex.Message}");
            }
        }


        public async Task<Response<int>> Update(Conta conta)
        {
            try
            {
                var cpf = conta.CPF.ToString();

                bool valido = ValidarCpf(cpf);
                if (!valido)
                {
                    return Response<int>.Error($"Empresa não encontrado para o CPF: {conta.CPF}");
                }
                

                using var conn = _db.GetConnection();

                var result = await conn.ExecuteAsync(
                   "UPDATE Conta SET nome = @Nome, email = @email, telefone = @telefone, cpf = @CPF," +
                   " numeroConta = @NumeroConta, agencia = @Agencia, endereco = @endereco WHERE id = @Id",
                   conta);

                return Response<int>.Ok(result);
            }
            catch (Exception ex)
            {
                return Response<int>.Error($"Erro ao atualizar conta com ID: {conta.Id}  {ex.Message}");
            }
        }

        public async Task<Response<bool>> Delete(int id)
        {
            try
            {

                var empresaExiste = await VerificarExistenciaconta(id);
                if (!empresaExiste)
                {
                    return Response<bool>.Error($"Conta com ID {id} não encontrada.");
                }

                using var conn = _db.GetConnection();

                var result = await conn.ExecuteAsync("DELETE FROM Conta WHERE id = @Id", new { Id = id });

                if (result == 0)
                {
                    return Response<bool>.Error($"Nenhuma conta encontrada com o ID: {id}");
                }

                return Response<bool>.Ok(true);

            }
            catch (Exception ex)
            {
                return Response<bool>.Error($"Erro ao deletar conta com ID: {id} {ex.Message}");
            }
        }


        public static bool ValidarConta(int numeroConta, int agencia, out string mensagemErro)
        {
            if (numeroConta <= 0)
            {
                mensagemErro = "O número da conta deve ser maior que zero.";
                return false;
            }

            if (agencia <= 0)
            {
                mensagemErro = "A agência deve ser maior que zero.";
                return false;
            }

            mensagemErro = "";
            return true;
        }

        public static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string temp = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(temp[i].ToString()) * mult1[i];

            int resto = soma % 11;
            int digito1 = (resto < 2) ? 0 : 11 - resto;

            temp += digito1;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(temp[i].ToString()) * mult2[i];

            resto = soma % 11;
            int digito2 = (resto < 2) ? 0 : 11 - resto;

            return cpf.EndsWith(digito1.ToString() + digito2.ToString());
        }


        public async Task<bool> VerificarExistenciaconta(int id)
        {
            try
            {
                var response = await GetById(id);
                return response.Success && response.Data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
