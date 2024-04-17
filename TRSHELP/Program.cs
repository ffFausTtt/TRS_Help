using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Microsoft.VisualBasic;

namespace TRSHELP
{
    class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient("6760722759:AAG44M97Z9ifv_u82UmaJoipbxO3IFrYbfk");
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                },
            };

            // Удаление webhook
            await _botClient.DeleteWebhookAsync();

            using (var cts = new CancellationTokenSource())
            {
                _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
                var me = await _botClient.GetMeAsync();
                Console.WriteLine($"{me.FirstName} запущен!");
                await Task.Delay(-1);
            }
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var messageFrom = update.Message;
                            var user = messageFrom.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {messageFrom.Text}");
                            string connectionString = "Data Source=ngknn.ru;Initial Catalog=22v_Ivanov;User ID=22V;Password=123";
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                int check = Convert.ToInt32(messageFrom.Text);
                                string sqlQuery = $"SELECT * FROM Task WHERE ID_Task = {check}";
                                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                {
                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string message = $"Номер: {reader["ID_Task"]}, Название: {reader["Title_Task"]}, Ссылка: {reader["Info"]}";
                                            long chatId = messageFrom.Chat.Id;
                                            await botClient.SendTextMessageAsync(chatId, message);
                                        }
                                    }
                                }
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}