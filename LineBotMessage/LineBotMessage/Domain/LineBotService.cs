﻿using System.Net.Http.Headers;
using LineBotMessage.Dtos;
using LineBotMessage.Enum;
using LineBotMessage.Providers;
using System.Text;
using LineBotMessage.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Net.WebSockets;
using isRock.LineBot;
using Microsoft.Extensions.FileProviders.Composite;

namespace LineBotMessage.Domain
{
    public class LineBotService
    {

        // 貼上 messaging api channel 中的 accessToken & secret
        private readonly string channelAccessToken = "";
        private readonly string channelSecret = "";

        private readonly string replyMessageUri = "https://api.line.me/v2/bot/message/reply";
        private readonly string broadcastMessageUri = "https://api.line.me/v2/bot/message/broadcast";


        private static HttpClient client = new HttpClient();
        private readonly JsonProvider _jsonProvider = new JsonProvider();
        private readonly LinebotAPIContext _context = new LinebotAPIContext();

        public LineBotService() { }

        public LineBotService(LinebotAPIContext context)
        {
            _context = context;
        }

      
        /// <summary>
        /// 接收 webhook event 處理
        /// </summary>
        /// <param name="requestBody"></param>

        public void ReceiveWebhook(WebhookRequestBodyDto requestBody)
        {
            dynamic replyMessage;
            foreach (var eventObject in requestBody.Events)
            {
                switch (eventObject.Type)
                {
                    case WebhookEventTypeEnum.Message:
                        if (eventObject.Message.Type == MessageTypeEnum.Text)
                            ReceiveMessageWebhookEvent(eventObject);
                        break;

                    case WebhookEventTypeEnum.Postback:
                        ReceivePostbackWebhookEvent(eventObject);
                        break;

                    case WebhookEventTypeEnum.Follow:
                        replyMessage = new ReplyMessageRequestDto<TextMessageDto>
                        {
                            ReplyToken = eventObject.ReplyToken,
                            Messages = new List<TextMessageDto>
                            {
                                new TextMessageDto(){Text = $"歡迎使用資訊服務組Line服務"}
                            }
                        };

                        ReplyMessageHandler(replyMessage);
                        break;

                }
            }
        }
        public void ReceivePostbackWebhookEvent(WebhookEventDto eventDto)
        {
            dynamic replyMessage = new ReplyMessageRequestDto<BaseMessageDto>();

            switch (eventDto.Postback.Data)
            {
                case "dataType=phoneSearch":
                    //分機查詢系統
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "分機查詢系統",
                                Template = new ButtonsTemplateDto
                                {
                                    ThumbnailImageUrl = "https://i.imgur.com/lAhzp6L.png?2",
                                    ImageAspectRatio = TemplateImageAspectRatioEnum.Rectangle,
                                    ImageSize = TemplateImageSizeEnum.Contain,
                                    Title = "親愛的用戶您好，歡迎您使用分機查尋系統",
                                    Text = "請選擇需要查尋的資訊",
                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Postback,
                                            Data = "dataType=staffs",
                                            Label = "職員",
                                            DisplayText = "職員"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Postback,
                                            Data = "dataType=departments",
                                            Label = "單位",
                                            DisplayText = "單位"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Postback,
                                            Data = "dataType=extentionNumbers",
                                            Label = "分機",
                                            DisplayText = "分機"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    break;

                case "dataType=staffs":
                    // 回傳使用職員名稱查詢的功能
                    replyMessage = CreateTextMessage("請輸入職員名稱，例:陳小明", eventDto);
                    break;

                case "dataType=departments":
                    // 回傳使用單位查詢的功能
                    replyMessage = CreateTextMessage("請輸入單位名稱，例：資訊服務組", eventDto);
                    break;

                case "dataType=extentionNumbers":
                    // 回傳使用分機查詢的功能
                    replyMessage = CreateTextMessage("請輸入分機4碼", eventDto);
                    break;

                case "dataType=job":
                    // 回傳業務職掌
                    replyMessage = CreateFlexCarouselFromFile(".\\JsonMessages\\richmenuJob.json", eventDto, "業務職掌");
                    break;

                case "dataType=service":
                    // 回傳服務項目
                    replyMessage = CreateFlexCarouselFromFile(".\\JsonMessages\\richmenuService.json", eventDto, "服務項目");
                    break;

                case "dataType=network":
                    // 回傳校園網路服務
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\richmenuNetwork.json", eventDto, "校園網路服務");
                    break;

                case "ex_1":
                    //回傳分機
                    replyMessage = CreateTextMessage("分機號碼: 3310 3321", eventDto);
                    break;

                case "ex_2":
                    //回傳分機
                    replyMessage = CreateTextMessage("分機號碼: 3311", eventDto);
                    break;

                case "ex_3":
                    //回傳分機
                    replyMessage = CreateTextMessage("分機號碼: 3313", eventDto);
                    break;

                case "ex_4":
                    //回傳分機
                    replyMessage = CreateTextMessage("分機號碼: 3312", eventDto);
                    break;

                case "email_1":
                    //回傳分機
                    replyMessage = CreateTextMessage("Email: norrith@ntus.edu.tw", eventDto);
                    break;

                case "email_2":
                    //回傳分機
                    replyMessage = CreateTextMessage("Email: rex@ntus.edu.tw", eventDto);
                    break;

                case "email_3":
                    //回傳分機
                    replyMessage = CreateTextMessage("Email: zonghao.xie@ntus.edu.tw", eventDto);
                    break;

                case "email_4":
                    //回傳分機
                    replyMessage = CreateTextMessage("Email: ", eventDto);
                    break;

                case "dataType=report":
                    //校園網路報修系統
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "校園網路報修系統",
                                Template = new ButtonsTemplateDto
                                {
                                    Title = "校園網路報修系統",
                                    Text = "若有校園網路、虛擬機及系統問題，煩請填寫以下報修單：",

                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "校園網路、虛擬機及系統報修單",
                                            Uri = "https://docs.google.com/forms/d/e/1FAIpQLSeAqe71-NZxWJoYnSfkwcI4tBif3Kty0FymuYTckqjv4-XOHg/viewform"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=survey":
                    //校園網路滿意度調查
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "校園網路滿意度調查",
                                Template = new ButtonsTemplateDto
                                {
                                    Title = "校園網路滿意度調查",
                                    Text = "完成報修服務，煩請填寫以下之校園網路、校務系統及入口網(含單一簽入)應用系統滿意度調查表：",

                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "校園網路滿意度調查",
                                            Uri = "https://docs.google.com/forms/d/e/1FAIpQLSeddapJFvZlOxDToZqpOHvmHjA4tVojQ3yBlr_9Dzhp1M1-Fg/viewform"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "校務系統滿意度調查",
                                            Uri = "https://docs.google.com/forms/d/e/1FAIpQLSeuwsG4fJWLgRY6sMplT8WonUTToDjXbjutGXpGeA2sryd4MA/viewform"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "校園入口網暨單一簽入滿意度調查表",
                                            Uri = "https://docs.google.com/forms/d/e/1FAIpQLSd9rZgZsfshkHdnBM53uJknMFQcNjbBgMxKuAmkD6Z16gY_LQ/viewform"
                                        }
                                        
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=ads":
                    //回傳廣告信和攻擊行為
                    replyMessage = CreateTextMessage("廣告信件：\r\n本校MailServer 只限校內IP，外界IP無法使用此MailServer寄\r\n發廣告信，不會成為廣告信轉發站；且本校MailServer有anti-spam機制，可防治廣告垃圾信之濫發。", eventDto);
                    break;

                case "dataType=abuse":
                    //回傳Abuse
                    replyMessage = CreateTextMessage("Abuse 和 copyright 帳號：   \r\n本校已建該帳號並有專人管理，資訊服務組同仁各依權責處理並為帳號管理人。\r\n \r\n管理人    帳號 \r\n汪新隆    abuse@ntus.edu.tw\r\n汪新隆    security@ntus.edu.tw", eventDto);
                    break;

                case "dataType=rules":
                    //回傳校園網路使用相關規範
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "校園網路使用相關規範",
                                Template = new ButtonsTemplateDto
                                {
                                    Text = "校園網路使用相關規範",
                                   

                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "校園網路使用規範要點",
                                            Uri = "https://www.ntus.edu.tw/upload/ckfinder/files/computer02.pdf"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "電子郵件管理要點",
                                            Uri = "https://www.ntus.edu.tw/js/upload/ckfinder/files/%E9%9B%BB%E5%AD%90%E9%83%B5%E4%BB%B6%E7%AE%A1%E7%90%86%E8%A6%81%E9%BB%9E.pdf"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=networkApply":
                    //回傳網路服務申請
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                             new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "網路服務申請",
                                Template = new ButtonsTemplateDto
                                {
                                    Text = "網路服務申請",

                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Postback,
                                            Label = "校園網路服務申請",
                                            Data = "campusNetworkApply"
                                        },

                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Postback,
                                            Label = "校園伺服器申請",
                                            Data = "campusServerApply"
                                        }
                                    }
                                }
                            }
                        }
                       
                    };

                    break;

                case "campusNetworkApply":
                    //校園網路服務申請
                    var json = File.ReadAllText(".\\JsonMessages\\campusNetworkApply.json");
                    replyMessage = new ReplyMessageRequestDto<ImagemapMessageDto>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<ImagemapMessageDto>
                        {
                            _jsonProvider.Deserialize<ImagemapMessageDto>(json) 
                        }
                    };

                    break;

                case "campusServerApply":
                    //校園伺服器申請
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                             new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "校園伺服器申請",
                                Template = new ButtonsTemplateDto
                                {
                                    Text = "ISMS-W-004-05校園伺服器設立變更取消申請表_V1.0",
                                    Title = "校園伺服器申請",
                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "PDF",
                                            Uri = "https://www.ntus.edu.tw/upload/ckfinder/files/ISMS-W-004-05%E6%A0%A1%E5%9C%92%E4%BC%BA%E6%9C%8D%E5%99%A8%E8%A8%AD%E7%AB%8B%E8%AE%8A%E6%9B%B4%E5%8F%96%E6%B6%88%E7%94%B3%E8%AB%8B%E8%A1%A8_V1_0.doc"
                                        },

                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "ODT",
                                            Uri = "https://www.ntus.edu.tw/upload/ckfinder/files/ISMS-W-004-05%E6%A0%A1%E5%9C%92%E4%BC%BA%E6%9C%8D%E5%99%A8%E8%A8%AD%E7%AB%8B%E8%AE%8A%E6%9B%B4%E5%8F%96%E6%B6%88%E7%94%B3%E8%AB%8B%E8%A1%A8_V1_0.odt"
                                        }
                                    }
                                }
                            }

                        }

                    };

                    break;

                case "dataType=information":
                    // 回傳資訊服務申請
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\information.json", eventDto, "資訊服務申請");
                    break;

                case "dataType=apiApply":
                    //api申請
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\apiApply.json", eventDto, "學校組織與人員API服務申請單");

                    break;

                case "dataType=authApply":
                    //單一簽入OAuth 2.0機制介接需求申請單
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\authApply.json", eventDto, "單一簽入OAuth 2.0機制介接需求申請單");

                    break;

                case "dataType=systemApply":
                    //資訊系統需求申請單
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\systemApply.json", eventDto, "資訊系統需求申請單");

                    break;

                case "dataType=accessApply":
                    //資訊系統使用權限申請單
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\accessApply.json", eventDto, "資訊系統使用權限申請單");

                    break;

                case "dataType=campusSystemApply":
                    //新一代校務資訊系統之系統維護申請單教學
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\campusSystemApply.json", eventDto, "新一代校務資訊系統之系統維護申請單教學");

                    break;

                case "dataType=pc":
                    //電腦設備預算專區
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\pc.json", eventDto, "電腦設備預算專區");

                    break;

                case "dataType=pcBudget112":
                    //112年度電腦設備預算編列
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "112年度電腦設備預算編列",
                                Template = new ButtonsTemplateDto
                                {
                                    Title = "112年度電腦設備預算編列",
                                    Text = "附件下載",


                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "112年各單位電腦經費預算分配表.ods",
                                            Uri = "https://lis.ntus.edu.tw/upload/library/attachment/021a1766bf13ef416a4bfca0aabdcb0b.ods"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "111-1圖委會_紀錄1.odt",
                                            Uri = "https://lis.ntus.edu.tw/upload/library/attachment/f2c82d49e1c514f56a61c1d5159f87a7.odt"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=pcBudget111":
                    //111年度電腦設備預算編列
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "111年度電腦設備預算編列",
                                Template = new ButtonsTemplateDto
                                {
                                    Title = "111年度電腦設備預算編列",
                                    Text = "請參考110學年度第1次圖書資訊委員會會議紀錄，預算編列詳如提案五：",


                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "會議決議",
                                            Uri = "https://www.ntus.edu.tw/upload/ckfinder/files/110-111%E5%9C%96%E5%A7%94%E6%84%B7%E7%B4%80%E9%8C%84.pdf"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "各單位電腦設備預算編列",
                                            Uri = "https://www.ntus.edu.tw/upload/ckfinder/files/111%E5%B9%B4%E5%90%84%E5%96%AE%E4%BD%8D%E9%9B%BB%E8%85%A6%E7%B6%93%E8%B2%BB%E9%A0%90%E7%AE%97%E5%88%86%E9%85%8D%E8%A1%A8.pdf"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=pcBudget110":
                    //110年度電腦設備預算編列
                    replyMessage = new ReplyMessageRequestDto<TemplateMessageDto<ButtonsTemplateDto>>
                    {
                        ReplyToken = eventDto.ReplyToken,
                        Messages = new List<TemplateMessageDto<ButtonsTemplateDto>>
                        {
                            new TemplateMessageDto<ButtonsTemplateDto>
                            {
                                AltText = "110年度電腦設備預算編列",
                                Template = new ButtonsTemplateDto
                                {                                    
                                    Text = "110年度電腦設備預算列表",


                                    Actions = new List<ActionDto>
                                    {
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "各單位電腦設備預算編列",
                                            Uri = "https://lis.ntus.edu.tw/index.php?act=download&ids=24280&path=https://lis.ntus.edu.tw/upload/library/attachment/e1833e1f2005cf941a35ea79ed950898.ods&title=110%E5%B9%B4%E5%BA%A6%E9%9B%BB%E8%85%A6%E8%A8%AD%E5%82%99%E9%A0%90%E7%AE%97%E7%B7%A8%E5%88%97"
                                        },
                                        new ActionDto
                                        {
                                            Type = ActionTypeEnum.Uri,
                                            Label = "教學單位主機汰換經費分配表",
                                            Uri = "https://lis.ntus.edu.tw/index.php?act=download&ids=24321&path=https://lis.ntus.edu.tw/upload/library/attachment/80579f86800a71fff91d78ea2b0db954.pdf&title=110%E5%B9%B4%E6%95%99%E5%AD%B8%E5%96%AE%E4%BD%8D%E4%B8%BB%E6%A9%9F%E6%B1%B0%E6%8F%9B%E7%B6%93%E8%B2%BB%E5%88%86%E9%85%8D%E8%A1%A8"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    break;

                case "dataType=docs":
                    //技術說明文件
                    replyMessage = CreateFlexBubbleFromFile(".\\JsonMessages\\docs.json", eventDto, "技術說明文件");

                    break;


                default:
                    break;
            }

            ReplyMessageHandler(replyMessage);
        }

        private ReplyMessageRequestDto<TextMessageDto> CreateTextMessage(string text, WebhookEventDto eventDto)
        {
            return new ReplyMessageRequestDto<TextMessageDto>
            {
                ReplyToken = eventDto.ReplyToken,
                Messages = new List<TextMessageDto>
                {
                    new TextMessageDto() { Text = text }
                }
            };
        }

        private ReplyMessageRequestDto<FlexMessageDto<FlexCarouselContainerDto>> CreateFlexCarouselFromFile(string filePath, WebhookEventDto eventDto, string altText)
        {
            var json = File.ReadAllText(filePath);
            return new ReplyMessageRequestDto<FlexMessageDto<FlexCarouselContainerDto>>
            {
                ReplyToken = eventDto.ReplyToken,
                Messages = new List<FlexMessageDto<FlexCarouselContainerDto>>
                {
                    new FlexMessageDto<FlexCarouselContainerDto>()
                    {
                        AltText = altText,
                        Contents = _jsonProvider.Deserialize<FlexCarouselContainerDto>(json)
                    }
                }
            };
        }

        private ReplyMessageRequestDto<FlexMessageDto<FlexBubbleContainerDto>> CreateFlexBubbleFromFile(string filePath, WebhookEventDto eventDto, string altText)
        {
            var json = File.ReadAllText(filePath);
            return new ReplyMessageRequestDto<FlexMessageDto<FlexBubbleContainerDto>>
            {
                ReplyToken = eventDto.ReplyToken,
                Messages = new List<FlexMessageDto<FlexBubbleContainerDto>>
                {
                    new FlexMessageDto<FlexBubbleContainerDto>()
                    {
                        AltText = altText,
                        Contents = _jsonProvider.Deserialize<FlexBubbleContainerDto>(json)
                    }
                }
            };
        }

        public void ReceiveMessageWebhookEvent(WebhookEventDto eventDto)
        {
            var replyMessage = new ReplyMessageRequestDto<TextMessageDto>();
            string messageText = "";

            switch (eventDto.Message.Type)
            {
                case MessageTypeEnum.Text:

                    // 將使用者輸入的字串存到 userInput 變數
                    string userInput = eventDto.Message.Text.Trim();

                    // 使用 LINQ 查詢資料庫中是否有對應的 ExtentionNumber、Staff 或 Department 記錄
                    var extentionnumber = _context.Staffs
                        .Include(e => e.StaffsDepartmentNavigation)
                        .FirstOrDefault(pext => pext.StaffsExtentionnumber.Equals(userInput));

                    var multiExtentionnumber = _context.StaffsMultiExtentionnumbers
                        .Include(e => e.StaffsDepartmentNavigation)
                        .FirstOrDefault(pext =>
                        pext.StaffsExtentionnumber1.Equals(userInput) ||
                        pext.StaffsExtentionnumber2.Equals(userInput) ||
                        pext.StaffsExtentionnumber3.Equals(userInput));

                    var staff = _context.Staffs
                        .Include(s => s.StaffsDepartmentNavigation)
                        .Where(pext => pext.StaffsName.Contains(userInput))
                        .ToList();

                    var staffMulti = _context.StaffsMultiExtentionnumbers
                        .Include(s => s.StaffsDepartmentNavigation)
                        .FirstOrDefault(pext => pext.StaffsName.Contains(userInput));


                    var dept = from department in _context.Departments
                               join staff_dept in _context.Staffs on department.Id equals staff_dept.StaffsDepartment into staffsGroup
                               from staff_dept in staffsGroup.DefaultIfEmpty()
                               join staffMulti_dept in _context.StaffsMultiExtentionnumbers on department.Id equals staffMulti_dept.StaffsDepartment into staffsMultiGroup
                               from staffMulti_dept in staffsMultiGroup.DefaultIfEmpty()
                               where department.DepartmentsName == userInput
                               select new { staff_dept, staffMulti_dept, department.DepartmentsName };


                    // 判斷是否找到對應的 PhoneExtentionNumber、Staff 或 Department 記錄
                    if (staff.Any() || staffMulti !=null)
                    {
                        messageText = "";
                        foreach (var staffcount in staff)
                        {
                            messageText += $"{staffcount.StaffsName}\n所屬單位:{staffcount.StaffsDepartmentNavigation.DepartmentsName}\n分機: {staffcount.StaffsExtentionnumber}\n\n";
                        }
                        string extNumbers = "";
                        if (staffMulti.StaffsExtentionnumber1 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber1 + " ";
                        }
                        if (staffMulti.StaffsExtentionnumber2 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber2 + " ";
                        }
                        if (staffMulti.StaffsExtentionnumber3 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber3 + " ";
                        }
                        messageText += $"{staffMulti.StaffsName}\n所屬單位:{staffMulti.StaffsDepartmentNavigation.DepartmentsName}\n分機: {extNumbers}\n\n";

                    }
                    else if (staffMulti != null)
                    {
                        string extNumbers = "";
                        if (staffMulti.StaffsExtentionnumber1 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber1 + " ";
                        }
                        if (staffMulti.StaffsExtentionnumber2 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber2 + " ";
                        }
                        if (staffMulti.StaffsExtentionnumber3 != null)
                        {
                            extNumbers += staffMulti.StaffsExtentionnumber3 + " ";
                        }
                        messageText = $"所屬單位: {staffMulti.StaffsDepartmentNavigation.DepartmentsName}\n分機號碼: {extNumbers}";
                    }
                    else if (extentionnumber != null)
                    {
                        messageText = $"{extentionnumber.StaffsName} \n所屬單位: {extentionnumber.StaffsDepartmentNavigation.DepartmentsName}";
                    }
                    else if (multiExtentionnumber != null)
                    {
                        messageText = $"{multiExtentionnumber.StaffsName} \n所屬單位: {multiExtentionnumber.StaffsDepartmentNavigation.DepartmentsName}";
                    }
                    else if (dept.Any())
                    {
                        messageText = "";
                        bool isExecuted = false;
                        foreach (var staffcount in dept)
                        {
                            if (!isExecuted && staffcount.staffMulti_dept != null)
                            {
                                isExecuted = true;
                                string extNumbers = "";
                                if (staffcount.staffMulti_dept.StaffsExtentionnumber1 != null)
                                {
                                    extNumbers += staffcount.staffMulti_dept.StaffsExtentionnumber1 + " ";
                                }
                                if (staffcount.staffMulti_dept.StaffsExtentionnumber2 != null)
                                {
                                    extNumbers += staffcount.staffMulti_dept.StaffsExtentionnumber2 + " ";
                                }
                                if (staffcount.staffMulti_dept.StaffsExtentionnumber3 != null)
                                {
                                    extNumbers += staffcount.staffMulti_dept.StaffsExtentionnumber3 + " ";
                                }
                                messageText += $"{staffcount.staffMulti_dept.StaffsName}\n分機: {extNumbers}\n\n";
                            }

                            messageText += $"{staffcount.staff_dept.StaffsName}\n分機: {staffcount.staff_dept.StaffsExtentionnumber}\n\n";
                        }
                    }
                    else
                    {
                        replyMessage = new ReplyMessageRequestDto<TextMessageDto>
                        {
                            ReplyToken = eventDto.ReplyToken,
                            Messages = new List<TextMessageDto>
                    {
                        new TextMessageDto(){Text = $"找不到分機號碼，職員名字或單位名稱為 {userInput}"}
                    }
                        };
                    }

                    if (!string.IsNullOrEmpty(messageText))
                    {
                        var messageDto = new TextMessageDto { Text = messageText };
                        // 將訊息物件加入回應訊息列表
                        replyMessage = new ReplyMessageRequestDto<TextMessageDto>
                        {
                            ReplyToken = eventDto.ReplyToken,
                            Messages = new List<TextMessageDto> { messageDto }
                        };
                    }

                    break;
            }

            ReplyMessageHandler(replyMessage);
        }

       
       
        /// <summary>
        /// 接收到廣播請求時，在將請求傳至 Line 前多一層處理，依據收到的 messageType 將 messages 轉換成正確的型別，這樣 Json 轉換時才能正確轉換。
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="requestBody"></param>
        public void BroadcastMessageHandler(string messageType, object requestBody)
        {
            string strBody = requestBody.ToString();
            dynamic messageRequest = new BroadcastMessageRequestDto<BaseMessageDto>();
            switch (messageType)
            {
                case MessageTypeEnum.Text:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<TextMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Sticker:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<StickerMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Image:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<ImageMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Video:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<VideoMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Audio:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<AudioMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Location:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<LocationMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.Imagemap:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<ImagemapMessageDto>>(strBody);
                    break;

                case MessageTypeEnum.FlexBubble:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<FlexMessageDto<FlexBubbleContainerDto>>>(strBody);
                    break;

                case MessageTypeEnum.FlexCarousel:
                    messageRequest = _jsonProvider.Deserialize<BroadcastMessageRequestDto<FlexMessageDto<FlexCarouselContainerDto>>>(strBody);
                    break;
            }
            BroadcastMessage(messageRequest);

        }

        /// <summary>
        /// 將廣播訊息請求送到 Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        public async void BroadcastMessage<T>(BroadcastMessageRequestDto<T> request)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken); //帶入 channel access token
            var json = _jsonProvider.Serialize(request);
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(broadcastMessageUri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(requestMessage);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// 接收到回覆請求時，在將請求傳至 Line 前多一層處理(目前為預留)
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="requestBody"></param>
        public void ReplyMessageHandler<T>(ReplyMessageRequestDto<T> requestBody)
        {
            ReplyMessage(requestBody);
        }

        /// <summary>
        /// 將回覆訊息請求送到 Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        public async void ReplyMessage<T>(ReplyMessageRequestDto<T> request)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken); //帶入 channel access token
            var json = _jsonProvider.Serialize(request);
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(replyMessageUri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(requestMessage);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

    }
}

