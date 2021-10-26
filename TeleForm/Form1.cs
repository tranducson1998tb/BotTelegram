using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Telegram.Bot;
using Telegram.Bot.Args;
using Newtonsoft.Json;
using System.IO;
using TeleForm.Model;
using System.Threading;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TeleForm
{
    public partial class Form1 : Form
    {
        static ITelegramBotClient botClient;
        static BotModel items;
        static List<ActionsModel> actions;
        static int count = 0;
        static String last_message = "";
        static int last_count = 0;
        static readonly List<String> amounts = new List<string> { "10", "20", "50", "100", "200", "500", "1000", "2000", "5000"  };
        static List<TransactionModel> transactionList = new List<TransactionModel>();
        static TransactionModel transactionModel;
        private readonly string _path = "D:/Project_telegram/TeleForm/TeleForm/data.json"; 
        public Form1()
        {
            InitializeComponent();
            using (StreamReader r = new StreamReader("D:/Project_telegram/TeleForm/TeleForm/data_example.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<BotModel>(json);
                actions = items.actions;

                //Console.WriteLine($"Hello, Bot ID is {items.botID}");
                botClient = new TelegramBotClient(items.botID);
                var me = botClient.GetMeAsync().Result;
                Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
                //count += 1;

                var cts = new CancellationTokenSource();

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                botClient.StartReceiving(
                    new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                    cts.Token);

                if (!System.IO.File.Exists(_path))
                {
                    createJsonFile();
                }
            }
        }

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                await botClient.SendTextMessageAsync(123, apiRequestException.ToString());
            }

            Console.WriteLine(exception.ToString());
        }


        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            if (update.Message.Type != MessageType.Text)
                return;
    
            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text;

            var replyKeyboardMarkupBuySell = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup(new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[] { actions[0].buy, actions[0].sell })
            {
                ResizeKeyboard = true
            };
            var replyKeyboardMarkupAmount = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup(
                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[][] {
                        new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[]{ amounts[0], amounts[1], amounts[2] },
                        new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[]{ amounts[3], amounts[4], amounts[5] },
                        new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[]{ amounts[6], amounts[7], amounts[8] }
                    }
            );

            

            if (text != null)
            {
                Console.WriteLine($"Received a message {text} in chat {chatId}.");
                if (amounts.Contains(text) && last_message == "Mua") {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Bạn muốn nhận AMAX về địa chỉ nào ?", replyMarkup: new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove());
                    transactionModel.Amount = int.Parse(text);
                    last_count = count;
                    updateData(transactionModel);
                } else
                {
                    if (amounts.Contains(text) && last_message == "Bán")
                    {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Vui lòng nhập số tài khoản nhận tiền của bạn ?", replyMarkup: new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove());
                        transactionModel.Amount = int.Parse(text);
                        last_count = count;
                        updateData(transactionModel);
                    }
                    else
                    {
                        switch (text)
                        {
                            case "Mua":
                                await botClient.SendTextMessageAsync(chatId: chatId, text: actions[0].how_much_buy, replyMarkup: replyKeyboardMarkupAmount);
                                last_message = text;
                                last_count = count;
                                count += 1;
                                transactionModel = new TransactionModel();
                                transactionModel.Type = text;
                                transactionModel.ID = count;
                                addData(transactionModel);
                                break;

                            case "Bán":
                                await botClient.SendTextMessageAsync(chatId: chatId, text: actions[0].how_much_sell, replyMarkup: replyKeyboardMarkupAmount);
                                last_message = text;
                                last_count = count;
                                count += 1;
                                transactionModel = new TransactionModel();
                                transactionModel.Type = text;
                                transactionModel.ID = count;
                                addData(transactionModel);
                                break;

                            default:
                                await botClient.SendTextMessageAsync(chatId: chatId, text: actions[0].choosen, replyMarkup: replyKeyboardMarkupBuySell);
                                break;
                        }
                    }
                }




            }
        }


        private void createJsonFile()
        {
            var trans = new TransactionModel {};
            ListTransactionModel listTransaction = new ListTransactionModel { };
            listTransaction.transactions = new List<TransactionModel>();
            listTransaction.transactions.Add(trans);
            var jsonWrite = JsonConvert.SerializeObject(listTransaction.transactions);
            using (StreamWriter writer = new StreamWriter(_path))
            {
                writer.Write(jsonWrite);
            }
        }

        private void updateData(TransactionModel transactionModel)
        {
            if (last_count == transactionModel.ID)
            {
                String existingFileContent = System.IO.File.ReadAllText(_path);

                List<TransactionModel> list = JsonConvert.DeserializeObject<List<TransactionModel>>(existingFileContent);
                list[last_count].Amount = transactionModel.Amount;
                //list.Add(transactionModel);
                var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);

                System.IO.File.WriteAllText(_path, convertedJson);
            }

            //var jsonWrite = JsonConvert.SerializeObject(transactionModel, Formatting.Indented);
            //using (StreamWriter writer = new StreamWriter(_path))
            //{
            //    writer.Write(jsonWrite);
            //}
        }

        private void addData(TransactionModel transactionModel)
        {
            String existingFileContent = System.IO.File.ReadAllText(_path);
            var list = JsonConvert.DeserializeObject<List<TransactionModel>>(existingFileContent);
            list.Add(transactionModel);
            var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);

            System.IO.File.WriteAllText(_path, convertedJson);
        }
    }
}
