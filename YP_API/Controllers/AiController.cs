using Microsoft.AspNetCore.Mvc;
using YP_API.Helpers;
using YP_API.Models;
using YP_API.Models.AIAPI;

namespace YP_API.Controllers
{
    public class UserQueryDto
    {
        // Текст вопроса от пользователя
        public string Prompt { get; set; }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {

        // Если ваш метод GetAnswer находится в статическом классе GigaChatService:
        // (Если он не статический, нужно внедрить сервис через конструктор)

        [HttpPost("ask")]
        public async Task<IActionResult> AskGigaChat([FromBody] UserQueryDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Prompt))
            {
                return BadRequest("Запрос не может быть пустым.");
            }

            try
            {
                var messages = new List<Request.Message>
                {
                    new Request.Message
                    {
                        role = "user",
                        content = input.Prompt
                    }
                };

                var token = await GigaChatHelper.GetToken(GigaChatHelper.ClientId, GigaChatHelper.AuthorizationKey);
                var response = await GigaChatHelper.GetAnswer(token, messages);

                if (response == null || response.choices == null || response.choices.Count == 0)
                {
                    return StatusCode(502, "Не удалось получить ответ от GigaChat API.");
                }

                // 4. Возврат ответа клиенту
                // Возвращаем только текст ответа, чтобы не перегружать клиента лишними данными
                return Ok(new
                {
                    answer = response.choices[0].message.content,
                    usage = response.usage // Можно вернуть и статистику токенов
                });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }





    [HttpGet("test-generation-real")]
        public async Task<IActionResult> TestGenerationWithMockData()
        {
            try
            {
                // 1. Создаем тестовый список ингредиентов (имитация того, что есть в холодильнике)
                var mockIngredients = new List<Ingredient>
        {
            new Ingredient { Name = "Куриная грудка", Unit = "кг" },
            new Ingredient { Name = "Рис", Unit = "кг" },
            new Ingredient { Name = "Помидоры", Unit = "шт" },
            new Ingredient { Name = "Сметана", Unit = "г" },
            new Ingredient { Name = "Чеснок", Unit = "зуб" }
        };

                // 2. Получаем токен (используя ваш метод GetToken)
                // ВАЖНО: Подставьте свои реальные RqUID и Authorization Key (AuthData)
                string rqUid = Guid.NewGuid().ToString();
                string authData = "ВАШ_BASE64_KEY_ЗДЕСЬ";

                string token = await GigaChatHelper.GetToken(GigaChatHelper.ClientId, GigaChatHelper.AuthorizationKey);

                if (string.IsNullOrEmpty(token))
                {
                    return StatusCode(500, "Ошибка получения токена (Token is null)");
                }

                // 3. Вызываем сервис генерации (предполагаем, что методы выше находятся в классе AiService)
                // Если методы в том же контроллере, просто вызываем их:
                var generatedMenu = await GigaChatHelper.GenerateAndParseMenuAsync(token, mockIngredients, 1);

                if (generatedMenu == null)
                {
                    return StatusCode(502, "GigaChat вернул пустой ответ или ошибка парсинга.");
                }

                // 4. Возвращаем результат
                return Ok(new
                {
                    Message = "Меню успешно сгенерировано на основе тестовых продуктов",
                    InputIngredients = mockIngredients.Select(i => i.Name),
                    Result = generatedMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка: {ex.Message}");
            }
        }
    }
}