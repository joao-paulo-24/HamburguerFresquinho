using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjetoLDS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebAPI.Data;
using WebAPI.Enums;
using WebAPI.Identity;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContaUtilizadorController : ControllerBase
    {
        private readonly BurgerShopContext context;
        public ContaUtilizadorController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;
        private const string TokenSecret = "ThisKeyIsExtremelySafeKeyAndThereIsNoDanger";
        private static readonly TimeSpan TokenLifeTime = TimeSpan.FromMinutes(15);


        /// <summary>
        /// Adiciona uma nova conta de utilizador. 
        /// JSON pede role mas o utilizador é sempre adicionado como cliente. A edição da role para primeiro
        /// utilizador administrador do software deve ser configurada diretamente na base de dados
        /// </summary>
        /// <returns> Retorna um objeto ContaUtilizador recém-criado e o status 201 (Created) se a operação for bem-sucedida ou BadRequest caso algum parametro nao seja cumprido. </returns>

        [HttpPost]
        public ActionResult<ContaUtilizador> AddConta(ContaUtilizador contaUtilizador)
        {

            var existingUser = context.ContaUtilizador.FirstOrDefault(u => u.Email == contaUtilizador.Email);

            // Validar se o email está em uso
            if (existingUser != null)
            {
                return BadRequest("Email já está em uso");
            }

            // Verificar se o nome de utilizador já existe
            var existingUserByUsername = context.ContaUtilizador.FirstOrDefault(u => u.Username == contaUtilizador.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest("Nome de utilizador já está em uso");
            }
            string hashedPassword = HashPassword(contaUtilizador.Password);
            contaUtilizador.Password = hashedPassword;
            ContaUtilizador newUser = new ContaUtilizador(contaUtilizador.Username, contaUtilizador.Password, contaUtilizador.Email, contaUtilizador.Role);

            context.ContaUtilizador.Add(newUser);
            context.SaveChanges();
            return CreatedAtAction(nameof(AddConta), new { id = contaUtilizador.Id }, contaUtilizador);
        }

        [HttpGet]
        public ActionResult<IEnumerable<ContaUtilizador>> GetContas()
        {

            if (context.ContaUtilizador == null)
            {
                return NotFound();
            }
            return Ok(context.ContaUtilizador.ToList());
        }

        /// <summary>
        /// Função responsável por verificar se existe a conta na base de dados e gerar o respetivo token
        /// </summary>
        /// <param name="user">utilizador que vai fazer a autenticação</param>
        /// <returns>Status 200 caso tudo corra bem, BadRequest caso algo não seja cumprido</returns>
        [HttpPost("token")]
        public IActionResult GenerateToken(
            [FromBody] ContaUtilizador user)
        {

            var existingUser = context.ContaUtilizador.FirstOrDefault(u => u.Email == user.Email);

            if (existingUser == null || VerifyPassword(user.Password, existingUser.Password) == false)
            {
                // Utilizador não encontrado na base de dados ou senha incorreta
                return BadRequest("Credenciais inválidas");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, existingUser.Email),
                new(JwtRegisteredClaimNames.Email, existingUser.Email),
                new("userid", existingUser.Id.ToString()),
                new("role", existingUser.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifeTime),
                Issuer = "https://localhost:7235",
                Audience = "https://localhost:7103",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);
            return Ok(jwt);
        }

        /// <summary>
        /// Função responsável por verificar se existe a conta na base de dados e gerar o respetivo token
        /// </summary>
        /// <param name="user">utilizador que vai fazer a autenticação</param>
        /// <returns>Status 200 caso tudo corra bem, BadRequest caso algo não seja cumprido</returns>
        [HttpPost("tokenLogged")]
        public IActionResult GenerateTokenLogged(
            [FromBody] ContaUtilizador user)
        {

            var existingUser = context.ContaUtilizador.FirstOrDefault(u => u.Email == user.Email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, existingUser.Email),
                new(JwtRegisteredClaimNames.Email, existingUser.Email),
                new("userid", existingUser.Id.ToString()),
                new("role", existingUser.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifeTime),
                Issuer = "https://localhost:7235",
                Audience = "https://localhost:7103",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);
            return Ok(jwt);
        }


        /// <summary>
        /// Função para fazer registo na aplicação
        /// </summary>
        /// <param name="newUser">Utilizador a ser registado na base de dados</param>
        /// <returns>Status 200 se tudo correr bem e o utilizador for adicionado à base de dados
        /// ou BadRequest caso algum dos parâmetros não seja cumprido</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] ContaUtilizador newUser)
        {

            var existingUser = context.ContaUtilizador.FirstOrDefault(u => u.Email == newUser.Email);

            // Validar se o email está em uso
            if (existingUser != null)
            {
                return BadRequest("Email já está em uso");
            }

            // Verificar se o nome de utilizador já existe
            var existingUserByUsername = context.ContaUtilizador.FirstOrDefault(u => u.Username == newUser.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest("Nome de utilizador já está em uso");
            }

            // Validar se o email está em branco ou nulo
            if (string.IsNullOrWhiteSpace(newUser.Email))
            {
                return BadRequest("Email não pode estar em branco ou nulo");
            }

            // Validar se nome de utilizador está em branco ou nulo
            if (string.IsNullOrWhiteSpace(newUser.Username))
            {
                return BadRequest("Nome de utilizador não pode estar em branco ou nulo");
            }

            // Validar se a password está em branco ou nula
            if (string.IsNullOrWhiteSpace(newUser.Email))
            {
                return BadRequest("A password não pode estar em branco ou nula");
            }

            // Validação para a senha ter pelo menos 6 caracteres
            if (string.IsNullOrWhiteSpace(newUser.Password) || newUser.Password.Length < 6)
            {
                return BadRequest("A senha deve ter pelo menos 6 caracteres");
            }

            string hashedPassword = HashPassword(newUser.Password);

            newUser.Password = hashedPassword;

            newUser.Role = Enums.EnumUserRole.Cliente;

            newUser.Pontos = 0;
            // Se tudo estiver válido, registe o novo utilizador
            context.ContaUtilizador.Add(newUser);
            context.SaveChanges();

            return Ok("Registo bem sucedido");
        }


        /// <summary>
        /// Inicia o processo de login usando o provedor de autenticação do Google.
        /// </summary>
        /// <param name="returnUrl">URL para redirecionar o utilizador após a autenticação bem-sucedida.</param>
        /// <returns> Um desafio de autenticação que redireciona o utilizador para a página de login do Google.</returns>
        [HttpGet("GoogleLogin")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        /// <summary>
        /// Responde à autenticação bem-sucedida do Google e realiza ações adicionais, como registrar o usuário se necessário.
        /// </summary>
        /// <returns> Retorna um objeto contendo informações do utilizador do Google ou uma resposta de erro em caso de falha na autenticação. </returns>
        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Falha na autenticação do Google");
            }

            var claims = authenticateResult.Principal.Claims;
            var googleUser = new
            {
                UserId = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
                DisplayName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                Role = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value
                // Adicione outras informações que você deseja extrair
            };

            var existingUser = context.ContaUtilizador.FirstOrDefault(u => u.Email == googleUser.Email);

            if (existingUser == null)
            {
                // Se o usuário não existir, registre-o na base de dados
                var newUser = new ContaUtilizador
                {
                    Email = googleUser.Email,
                    Username = googleUser.DisplayName,
                    Role = Enums.EnumUserRole.Cliente,
                };

                context.ContaUtilizador.Add(newUser);
                context.SaveChanges();
            }

            return Ok(googleUser);
        }

        /// <summary>
        /// Encripta uma password
        /// </summary>
        /// <param name="password">password a ser encriptada</param>
        /// <returns>a password encriptada</returns>
        private string HashPassword(string password)
        {
            // Configuração do algoritmo PBKDF2
            int saltSize = 16; // Tamanho do "salt"
            int iterations = 10000; // Número de iterações
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltSize, iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32); // 32 bytes para a hash
                byte[] salt = pbkdf2.Salt;

                // Combinação do "salt" com a hash
                byte[] hashBytes = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, hashBytes, 0, salt.Length);
                Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

                // Convertendo para uma representação de string (pode ser armazenada no banco de dados)
                string hashedPassword = Convert.ToBase64String(hashBytes);

                return hashedPassword;
            }

        }

        /// <summary>
        /// Verifica se a senha inserida e a encriptada coincidem
        /// </summary>
        /// <param name="enteredPassword">Senha inserida</param>
        /// <param name="storedHashedPassword">Senha encriptada na base de dados</param>
        /// <returns>True se as senhas coincidirem, False caso contrário</returns>
        private bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {

            // Converta a senha armazenada em bytes
            byte[] storedHashBytes = Convert.FromBase64String(storedHashedPassword);

            // Extraia o "salt" dos bytes armazenados
            byte[] salt = new byte[16];
            Array.Copy(storedHashBytes, 0, salt, 0, 16);

            // Derive a senha usando o "salt" armazenado
            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000))
            {
                byte[] enteredHash = pbkdf2.GetBytes(32); // 32 bytes para a hash

                // Compara as hashes derivadas
                for (int i = 0; i < 32; i++)
                {
                    if (enteredHash[i] != storedHashBytes[i + 16])
                        return false;
                }
            }

            // Se chegou até aqui, as senhas coincidem
            return true;
        }


        // Método para validar o e-mail
        private bool IsValidEmail(string email)
        {
            // Define a expressão regular para validar o e-mail
            string pattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";

            // Usa a classe Regex para verificar se o e-mail corresponde ao padrão
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        [HttpPut("{id}/Update")]
        public ActionResult Update(int id, ContaUtilizador user)
        {
            var contaUtilizador = context.ContaUtilizador.Find(id);

            if (contaUtilizador == null)
            {
                return NotFound(); // Retornar 404 Not Found se a conta não existir
            }

            bool usernameExists = context.ContaUtilizador.Any(u => u.Username == user.Username && u.Id != id);

            // Validar se o username está em uso
            if (usernameExists)
            {
                return BadRequest("Username já está em uso");
            }

            // Validar se nome de utilizador está em branco ou nulo
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("Nome de utilizador não pode estar em branco ou nulo");
            }

            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6)
            {
                return BadRequest("A senha deve ter pelo menos 6 caracteres");
            }

            bool emailExists = context.ContaUtilizador.Any(u => u.Email == user.Email && u.Id != id);

            // Validar se o email está em branco ou nulo
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("Email não pode estar em branco ou nulo");
            }

            // Validar se o email está em uso
            if (emailExists)
            {
                return BadRequest("Email já está em uso");
            }

            // Verifica se o novo e-mail é válido usando a expressão regular
            if (!IsValidEmail(user.Email))
            {
                return BadRequest("O novo e-mail não é válido.");
            }


            contaUtilizador.Username = user.Username;
            contaUtilizador.Password = user.Password;
            contaUtilizador.Email = user.Email;


            string hashedPassword = HashPassword(contaUtilizador.Password);

            contaUtilizador.Password = hashedPassword;
            context.SaveChanges();

            return Ok(contaUtilizador);
        }

        /// <summary>
        /// Atualiza a role de um utilizador pelo seu id
        /// </summary>
        /// <param name="id">id do utilizador a ser alterado</param>
        /// <param name="role">nova role para o utilizador</param>
        /// <returns>NotFound caso o utilizador não exista, Ok success caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("UpdateRole/{id}")]
        public IActionResult UpdateRole(int id, EnumUserRole role)
        {

            ContaUtilizador contaUtilizador = context.ContaUtilizador.Find(id);

            if (contaUtilizador == null)
            {
                return NotFound();
            }

            contaUtilizador.Role = role;
            context.ContaUtilizador.Entry(contaUtilizador).State = EntityState.Modified;
            context.SaveChanges();

            return Ok(contaUtilizador);
        }
    }


}
