using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ValidacaoCPF;

public class Function1(ILogger<Function1> logger)
{
    [Function("fnvalidacpf")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous,"post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        if(data == null || data.cpf == null || !ValidacaoCPF((string)data.cpf))
        {
            return new BadRequestObjectResult("Invalid CPF.");
        }
        var responseMessage = "CPF valido e limpo na receita federal, e não consta na base de dados de débitos";

        return new OkObjectResult(responseMessage);
    }

    public static bool ValidacaoCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove caracteres não numéricos
        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        // CPF deve ter 11 dígitos
        if (cpf.Length != 11)
            return false;

        // Permite CPFs com todos os dígitos iguais (ex: 00000000000)
        // Se quiser bloquear, descomente a linha abaixo:
        // if (cpf.Distinct().Count() == 1) return false;

        // Calcula os dígitos verificadores
        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCpf += digito1;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        string digitosVerificadores = cpf.Substring(9, 2);
        string digitosCalculados = $"{digito1}{digito2}";

        return digitosVerificadores == digitosCalculados;
    }
}