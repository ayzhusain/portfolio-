using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Security.Cryptography;
using System.Resources;
using Gordion.Payment;


namespace WindowsFormsApplication1
{
    public partial class BlekingeTrafiken : Form
    {

        enum GUI_STATE { TICKET_GUI, TERMINAL_GUI };
        GUI_STATE guiState;

        private ButtonTimer yesButtonPresser;

        const decimal vuxen = 24.00m;
        const decimal barn = 14.00m;
        const decimal familj = 43.00m;

        private int noofVuxen = 0;
        private int noofBarn = 0;
        private int noofFamilj = 0;

        private decimal totalprise;

        private Gordion.Payment.GPayment m_paymentProvider;
        private int kiosk_ID = 1605;

        private string kiosk_IP = "127.0.0.1";
        private int kiosk_PORT = 6001;
        public int receipt_ID;



        //Language Flags
        bool transflags = false;
        int transCode = 48;

        private DataTable m_dtStatus = new DataTable("Status");
        private bool m_bInit = true;

        private int textFlag = 0;

        private PaymentState paymentState;
        private int finishedCounter;

        public BlekingeTrafiken()
        {
            InitializeComponent();

            InitPayment(GPayment.PaymentProvider.NetsBAXI);
            m_bInit = false;
            guiState = GUI_STATE.TICKET_GUI;
            transCode = 48;
            yesButtonPresser = new ButtonTimer(this, PressYesButton);
        }

        private PaymentTexts englishTexts, swedishTexts, polishTexts;

        private void Form1_Load(object sender, EventArgs e)
        {
            if (m_bInit) return;

            if (m_paymentProvider == null)
                InitPayment(GPayment.PaymentProvider.NetsBAXI);

            m_paymentProvider.PspInfo.IP = kiosk_IP;
            m_paymentProvider.PspInfo.NetPort = kiosk_PORT;

            m_paymentProvider.PspInfo.ComPort = -1;
            m_paymentProvider.PspInfo.AccountId = "";
            m_paymentProvider.PspInfo.TerminalId = "77175302";
            //m_paymentProvider.PspInfo.TerminalId = "33136452";
            m_paymentProvider.PspInfo.AccountId = "64212353";


            if (m_paymentProvider.PspInfo.IP != "")
            {
                Status(eStatusType.Info, 0, string.Format("Connecting to IP {0}:{1}", m_paymentProvider.PspInfo.IP, m_paymentProvider.PspInfo.NetPort));
            }
            else if (m_paymentProvider.PspInfo.IP == "" && m_paymentProvider.PspInfo.NetPort != -1)
            {
                Status(eStatusType.Info, 0, string.Format("Listening on Port {0}", m_paymentProvider.PspInfo.NetPort));
            }
            else if (m_paymentProvider.PspInfo.ComPort != -1)
            {
                Status(eStatusType.Info, 0, string.Format("Connecting to COM {0}", m_paymentProvider.PspInfo.ComPort));
            }
            else
            {
                MessageBox.Show("Please enter either a IP and port, just port or a COM port.");
                return;
            }
            PaymentInit oRes = m_paymentProvider.InitPayment();
            //SettleResult sRes = m_paymentProvider.Settle();

            switch (oRes)
            {
                case PaymentInit.PAYINIT_PIN_FIND_COM_FAILED:
                case PaymentInit.PAYINIT_PINPAD_NOANSWER:
                case PaymentInit.CONFIG_ERROR:
                    Status(eStatusType.Info, 0, string.Format("Could not find the Terminal! [{0}] Error: {1}", oRes, m_paymentProvider.Data.Error));

                    break;

                case PaymentInit.ERROR:
                    Status(eStatusType.Info, 0, string.Format("PSP Error! [{0}] Error: {1}", oRes, m_paymentProvider.Data.Error));

                    break;

                case PaymentInit.PAYINIT_OK:
                    Status(eStatusType.Info, 0, "Connected to terminal.");
                    m_bInit = true;
                    break;


                default:
                    Status(eStatusType.Info, 0, string.Format("Unknown Error! [{0}] Error: {1}", oRes, m_paymentProvider.Data.Error));

                    break;

            }

            //Custom Font... Bermeno
            //PrivateFontCollection pfc = new PrivateFontCollection();
            //pfc.AddFontFile("C:/Users/Ayazhussain/Desktop/BiljittTerminalData/BarmenoFull/BFM_____.TTF");
            ////pfc.AddFontFile("WindowsFormsApplication1/WindowsFormsApplication1/Resources/BFB_____.PFM");
            ////pfc.AddFontFile(Properties.Resources.ResourceManager.GetString("C:\Users\Ayazhussain\Documents\Visual Studio 2015\Projects\WindowsFormsApplication1\WindowsFormsApplication1\Resources\BFB_____.PFM"));
            //foreach (Control c in this.Controls)
            //{
            //    //c.Font = new Font(pfc.Families[0], 16, FontStyle.Regular);
            //    c.Font = new Font(pfc.Families[0], c.Font.Size, c.Font.Style);
            //    barnBtnPrisLable.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    barntLb.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    familjBtnPrisLable.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    familjtLb.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    groupBox1.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    TotalPristLb.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    VuxentLb.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);

            //    label1.Font = new Font(pfc.Families[0], 36, FontStyle.Regular);
            //    label2.Font = new Font(pfc.Families[0], 24, FontStyle.Regular);
            //    lbHeader.Font = new Font(pfc.Families[0], 20, FontStyle.Regular);
            //    lbRow1.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    lbRow2.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //    lbRow3.Font = new Font(pfc.Families[0], 13, FontStyle.Regular);
            //}

            //Button Price Lables
            vuxenBtnPrisLable.Text = vuxen.ToString() + " SEK";
            barnBtnPrisLable.Text = "7 - 19 år" + "\n" + barn.ToString() + " SEK";
            familjBtnPrisLable.Text = "2 Vuxen + 3 Barn " + "\n" + familj.ToString() + " SEK";

            InitializeTranslations();
            //ReplaceFonts();
        }

            //private void ReplaceFonts()
            //{
            //    PrivateFontCollection pfc = new PrivateFontCollection();
            //    pfc.AddFontFile("C:/Users/Ayazhussain/Desktop/BiljittTerminalData/BarmenoFull/BFB_____.ttf");

            //    ReplaceFont(this.Controls, pfc.Families[0]);

            //}
            //private void ReplaceFont(ControlCollection oCtrls, Font oFont)
            //{

            //    foreach (Control c in oCtrls)
            //    {
            //        c.Font = new Font(oFont, c.Font.Size, c.Font.Style);

            //        if (c.Controls.Count > 0)
            //            ReplaceFont(c.Controls, oFont);
            //    }
            //}
        private void InitializeTranslations()
        {
            englishTexts = new PaymentTexts(44);
            swedishTexts = new PaymentTexts(46);
            polishTexts = new PaymentTexts(48);

        }

        private void Status(object info, int v1, string v2)
        {
            MessageBox.Show("I am Status" + info + v1 + v2);

        }
        private enum eStatusType
        {
            Info = 0,
            Log,
            ProduceProduct,
            Database
        }

        private String GetReceiptHeader()
        {

            StringBuilder sHeadertext = new StringBuilder("BILJETT / BILET / TICKET\n24 TIM KARLSKRONA\n");
            if (noofBarn > 0)
            {
                sHeadertext.Append("Barn / Dziecko / Child\n");
            }
            if (noofVuxen > 0)
            {
                sHeadertext.Append("Vuxen / Dorosły / Adult\n");
            }
            if (noofFamilj > 0)
            {
                sHeadertext.Append("Familj / Rodzina / Family\n");
            }
            sHeadertext.Append(string.Format("\n{0} SEK\n\nGiltig Till\n{1} kl {2}\n-----------------------------------------", totalprise, DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("HH:mm")));
            return sHeadertext.ToString();
        }

        //private String GetReceiptFooter()
        //{
        //    StringBuilder sFootertext = new StringBuilder("-----------------------------------------\n\nBlekingetrafiken\n\nwww.blekingetrafiken.se\nValhallavägen 1 \n33 140  Karlskrona\n0455 569 00\n\n-----------------------------------------");

        //    return sFootertext.ToString();

        //}


        //Payment Method 
        private void InitPayment(GPayment.PaymentProvider paymentProvider)
        {
            //New Payment
            m_paymentProvider = new GPayment(paymentProvider, kiosk_ID, LogLevel.LOG_DETAILED);

            m_paymentProvider.PspInfo.TerminalId = "33136452";
            m_paymentProvider.TopMost = false;

            //PSP data
            //m_paymentProvider.PspInfo.
            //m_paymentProvider.PspInfo.TerminalId = "33136452";
            //m_paymentProvider.PspInfo.AccountId = "64212353";
            //m_paymentProvider.PspInfo.IP = kiosk_IP;
            //m_paymentProvider.PspInfo.NetPort = kiosk_PORT;

            //PSP Log Directory
            if (!Directory.Exists(Application.StartupPath + "\\Logs"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\Logs");
            }
            m_paymentProvider.PspInfo.LogPath = Application.StartupPath + "\\Logs";
            m_paymentProvider.Data.LogLevel = LogLevel.LOG_DEBUGMODE;

            //Card choice
            m_paymentProvider.Cards = new AcceptedCards(true, true, true,
                    false, false,
                    false, false,
                    false, false,
                    false, false, false);

            //Choose default language of the interface
            //Change the texts if you have any other texts to present
            //Texts can also be changed right before payment if you need to change it on the fly.
            m_paymentProvider.Texts = new PaymentTexts(46);

            //Choose the default language to print on the receipts, this should just be set upon initialization 
            //to not confuse customers with multiple languages on the receipts.
            m_paymentProvider.Receipts.ReceiptLanguage = 46;

            //Receipt data
            m_paymentProvider.Receipts.PrinterName = "CUSTOM VKP80 II";
            m_paymentProvider.Receipts.PrintCustomerToPrinter = true;

            

            string ftagNamn = "Blekingetrafiken";
            string ftageAdd = "";
            string ftagCity = "www.blekingetrafiken.se";
            string ftagFon = "+46 0455 569 00";

            m_paymentProvider.Receipts.SetMerchantData(ftagNamn,ftageAdd,ftagCity,ftagFon,"");


            //Skinning example
            //m_paymentProvider.Skin.BackgroundImage = new Bitmap("bg_no_flags.jpg");
            //m_paymentProvider.Skin.BackColor = Color.White;
            //m_paymentProvider.Skin.ErrorColor = Color.Red;
            //m_paymentProvider.Skin.ForeColor = Color.Black;
            //m_paymentProvider.Skin.ButtonForeColor = Color.Purple;
            //m_paymentProvider.Skin.HeaderColor = Color.Black;

            //Intitialize Events

            //Critical, sets receipt number
            m_paymentProvider.ReceiptEvent += new ReceiptEventHandler(m_paymentProvider_ReceiptEvent);


            ////Write tickets, produce slips, open gates or load giftcards here
            m_paymentProvider.ProduceProductEvent += new ProduceProductEventHandler(m_paymentProvider_ProduceProductEvent);

            ////Payment is complete and the form is closed
            m_paymentProvider.PaymentCompleteEvent += new PaymentCompleteEventHandler(m_paymentProvider_PaymentCompleteEvent);


            //////PSP logs regarding voids, purchases etc
            m_paymentProvider.DatabaseEvent += new DatabaseEventHandler(m_paymentProvider_DatabaseEvent);

            //////Payment logs regarding errors etc
            m_paymentProvider.DatabaseLogEvent += new DatabaseLogEventHandler(m_paymentProvider_DatabaseLogEvent);

            ////A receipt has been printed, use this to store it
            m_paymentProvider.PrintReceiptEvent += new PrintReceiptEventHandler(m_paymentProvider_PrintReceiptEvent);

            ////Cardcodes from the PSP is shown here
            ////With Samport dll is is not possible to get card data anymore.
            m_paymentProvider.CardCodeEvent += new CardCodeEventHandler(m_paymentProvider_CardCodeEvent);

            ////Use this to produce a custom interface
            m_paymentProvider.InfoEvent += new InfoEventHandler(m_paymentProvider_InfoEvent);

            ////Handle activation of pinpad
            m_paymentProvider.ActivationEvent += new ActivationEventHandler(m_paymentProvider_ActivationEvent);

            ////MouseHoverHandler
            //btYes.MouseMove += new MouseEventHandler(change_Btn_Color);
            //btNo.MouseMove += new MouseEventHandler(change_Btn_Color);
            //btnVuxen.MouseMove += new MouseEventHandler(change_Btn_Color);
            //button2.MouseMove += new MouseEventHandler(change_Btn_Color);
            //button3.MouseMove += new MouseEventHandler(change_Btn_Color);
            //familjBtnPrisLable.MouseMove += new MouseEventHandler(change_Btn_Color);
            //barnBtnPrisLable.MouseMove += new MouseEventHandler(change_Btn_Color);




        }

        void PressYesButton(object sender, EventArgs e)
        {
            m_paymentProvider.AnswerPositive();
        }
        /// <summary>
        /// Custom interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_paymentProvider_InfoEvent(object sender, InfoEventArgs e)
        {

            switch (e.Type)
            {
                //Disable Buttons
                case InfoEventType.DisableButtons:
                    btYes.Enabled = false;
                    btNo.Enabled = false;
                    break;

                //Enable Buttons
                case InfoEventType.EnableButtons:
                    btYes.Enabled = true;
                    btNo.Enabled = true;
                    if(paymentState == PaymentState.PS_FINISHED && --finishedCounter == 0)
                    {
                        yesButtonPresser.Start();
                    }
                    break;

                //State Changed
                case InfoEventType.BeforeStateChange:
                    paymentState = e.State;
                    ImageHandling();
                    if(paymentState == PaymentState.PS_START)
                    {
                        yesButtonPresser.Start();
                    }
                    break;
                case InfoEventType.AfterStateChange:
                case InfoEventType.Aborted:
                    break;

                //Texts changed
                case InfoEventType.TextChanged:

                    //textHandlingOnScreen();
                    lbHeader.Text = e.Header;
                    lbRow1.Text = e.Row1;
                    lbRow2.Text = e.Row2;
                    lbRow3.Text = e.Row3;

                    if (pbImage.Visible)
                    {
                        //pbImage.Image = e.Image;

                    }

                    btYes.Visible = false;
                    btNo.Visible = false;

                    if (!String.IsNullOrEmpty(e.YesText))
                    {
                        btYes.Text = e.YesText;
                        btYes.Visible = true;
                    }

                    if (!String.IsNullOrEmpty(e.NoText))
                    {
                        btNo.Text = e.NoText;
                        btNo.Visible = true;
                    }

                    lbPin.Text = "";
                    lbCountDown.Text = "";
                    break;

                //Countdown to inactivity
                case InfoEventType.CountDown:
                    if (e.CountDown <= 15) lbCountDown.Text = e.CountDown.ToString();
                    break;

                //Pin pressed, value in Row1
                case InfoEventType.PinKey:
                    lbPin.Text = e.Row1;
                    break;

            }
        }


        private void m_paymentProvider_PaymentCompleteEvent(object sender, PaymentCompleteEventArgs e)
        {
            MessageBox.Show("I am m_paymentProvider_PaymentCompleteEvent");

            resetTickets();
            pbImage.Hide();
            pbOurs.Hide();
            pbCardTypeImage.Hide();
            guiState = GUI_STATE.TICKET_GUI;
            loadmaininterface();
            resetTextfunction();
            m_paymentProvider.Dispose();
        }

        private void resetTextfunction()
        {
            lbHeader.Text = "";
            lbRow1.Text = "";
            lbRow2.Text = "";
            lbRow3.Text = "";
        }

        private void loadmaininterface()
        {

            //button3.Show();
            //button2.Show();
            //btnVuxen.Show();
            //familjBtnPrisLable.Show();
            //barnBtnPrisLable.Show();
            //vuxenBtnPrisLable.Show();

            pbOurs.Hide();
            Visible_Buttons();


            guiState = GUI_STATE.TICKET_GUI;
            switch (transCode)
            {
                case 44:
                    MainEnglishTranslation();
                    break;
                case 46:
                    MainSwedishTranslation();
                    break;
                default:
                    MainPolishTranslation();
                    break;
            }
        }

        private ProduceProductResult m_paymentProvider_ProduceProductEvent(object sender, ProduceProductEventArgs e)
        {
            ProduceProductResult nRet = ProduceProductResult.PRODUCE_PRODUCT_OK;

            
            m_paymentProvider.Receipts.SetHeader(GetReceiptHeader(), "Köpet Avbrutet!", "Köpet Medges Ej!");
            m_paymentProvider.Receipts.SetFooter("Välkommen åter!", "Försök gärna igen!", "Tyvärr, försök igen!");
            
            //Display info text
            //m_paymentProvider.SetText("Test", "You can change the text in the ProduceProduct Event", "", "This is an info text.", " ", true, true);
            //DialogResult oRes = MessageBox.Show("Do you want to accept the transaction?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            Status(eStatusType.Log, 0, "Product Produced OK.");

            return nRet;
        }



        /// <summary>
        /// Payment is requesting a receipt id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        int m_paymentProvider_ReceiptEvent(object sender, EventArgs e)
        {
            //int nRet = (int)udRecipe.Value;
            //udRecipe.Value++;
            //Status(eStatusType.Info, 0, string.Format("Setting Receipt ID: {0}", nRet));
            //return nRet;

            int nRet = 0; // receipt_ID++;  //we have to save receipt id to a file, so it will not become 0 if we restart the program... somwhere in file.. 
            return nRet = setReceipt(); ;
        }

        private int setReceipt()
        {
            receipt_ID++;
            return receipt_ID;
        }

        /// <summary>
        /// Payment wants us to store a psp data event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        bool m_paymentProvider_DatabaseEvent(object sender, DatabaseEventArgs e)
        {

            string sReceiptId = m_paymentProvider.Data.ReceiptId;
            int nReceiptId = 0;
            try { nReceiptId = int.Parse(sReceiptId); }
            catch { }

            decimal fAmount = m_paymentProvider.Data.Products.TotalPrice;
            string sCardCode = m_paymentProvider.Data.ReceiptData.CardMaskedPAN;
            string sName = m_paymentProvider.Data.ReceiptData.CardHolderName;
            string sCardType = m_paymentProvider.Data.ReceiptData.CardProductName;
            string sPayMode = m_paymentProvider.Data.ReceiptData.AccountType;
            string sAuthorizationNo = m_paymentProvider.Data.AuthorizationNo;
            string sNetErrorMessage = m_paymentProvider.Data.Error;
            string sResult = m_paymentProvider.Data.AuthorizeDeniedPSPDetails;

            PayAccountCredit nPayMode = PayAccountCredit.PAY_NONE;
            DbEvent nEventId = e.DbEvent;

            switch (sPayMode.ToLower())
            {
                case "1":
                    nPayMode = PayAccountCredit.PAY_ACCOUNT;
                    break;

                case "2":
                    nPayMode = PayAccountCredit.PAY_CREDIT;
                    break;
            }

            // Datafileds depending on action
            switch (e.DbEvent)
            {
                case DbEvent.DB_AUTHORIZE_OK:
                    break;

                case DbEvent.DB_AUTHORIZE_FAILED:
                    Status(eStatusType.Log, (int)nEventId, string.Format("ReceiptId: {0}  sValueA: {1}  Result:{2}{3} ", nReceiptId,
                            "GiftCard::PurchaseCard", "Purchase denied, response code:", m_paymentProvider.Data.ReceiptData.ResponseCode));
                    break;

                case DbEvent.DB_VOID_OK:
                    Status(eStatusType.Log, (int)nEventId, string.Format("ReceiptId: {0}  sValueA: {1}  Result:{2} ", nReceiptId,
                            "GiftCard::PurchaseCard", "Purchase failed, the money was refunded"));

                    break;

                case DbEvent.DB_VOID_FAILED:
                case DbEvent.DB_VOID_FAILED_NETWORK:
                case DbEvent.DB_VOID_FAILED_OTHER:
                    Status(eStatusType.Log, (int)nEventId, string.Format("ReceiptId: {0}  sValueA: {1}  Result:{2} ", nReceiptId,
                            "GiftCard::PurchaseCard", "Purchase failed, could not refund the money."));

                    break;

                case DbEvent.DB_PURCHASE_OK:
                    Status(eStatusType.Log, (int)nEventId, string.Format("ReceiptId: {0}  sValueA: {1}  Result:{2} ", nReceiptId,
                           "GiftCard::PurchaseCard", "Purchase completed OK"));

                    break;
            }

            Status(eStatusType.Database, (int)nEventId, string.Format("ReceiptId: {0}  Amount: {1}  CardCode: {2}  Name: {3}  CardType: {4}  PayMode: {5}  AuthNo: {6}  EventId: {7}  Result: {8}  ValueA: {9}  ValueB: {10}",
                 nReceiptId, fAmount, sCardCode, sName, sCardType, nPayMode, sAuthorizationNo, nEventId, sResult, sNetErrorMessage, ""));

            return true;
        }

        /// <summary>
        /// Payment is requesting that we write to the log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_paymentProvider_DatabaseLogEvent(object sender, DatabaseLogEventArgs e)
        {
            try
            {
                string sLog = string.Format("LogTypeId: {0}  Value1: {1}  Value2:{2}  ValueA:{3}  ValueB:{4}", e.LogTypeId, e.Value1, e.Value2, e.ValueA, e.ValueB);
                Status(eStatusType.Log, e.Value1, sLog);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handle receipts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        bool m_paymentProvider_PrintReceiptEvent(object sender, EventArgs e)
        {
            //Save the receipt
            m_paymentProvider.Receipts.CustomerReciept.Save(string.Format("{0}_{1}.png", DateTime.Today.ToString("yyyy-MM-dd"), receipt_ID));

            //Or use the data in m_paymentProvider.Data.ReceiptData to print your own if you set m_paymentProvider.Receipts.PrintCustomerToPrinter = false;
            //printing
            //printReceipt();
            return false;
        }

        private void printReceipt()
        {
            Receipts.PrinterState oState = Receipts.PrinterState.Unknown;
            if (oState == 0)
            {
                Receipts.PrinterStatus oStatus = m_paymentProvider.Receipts.GetStatus(out oState);

                Status(eStatusType.Log, 1, string.Format("Printer Name: {0}  Status: {1}  State:{2} ", m_paymentProvider.Receipts.SelectedPrinter, oStatus, oState));
                //MessageBox.Show("Printer Name: {0}  Status: {1}  State:{2} "+ m_paymentProvider.Receipts.SelectedPrinter + m_paymentProvider.Receipts.GetStatus(), m_paymentProvider.Receipts.GetStatus(Receipts.PrinterState.));
            }
            else {
                Receipts.PrinterStatus oStatus = m_paymentProvider.Receipts.GetStatus(out oState);

                Status(eStatusType.Log, 1, string.Format("Printer Name: {0}  Status: {1}  State:{2} ", m_paymentProvider.Receipts.SelectedPrinter, oStatus, oState));
            }


        }
        /// <summary>
        /// We got a cardcode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        bool m_paymentProvider_CardCodeEvent(object sender, CardCodeEventArgs e)
        {
            if (e.RemoveCard)
            {
                Status(eStatusType.Info, 0, "Card Inserted: Please remove it again.");
            }
            else
            {
                Status(eStatusType.Info, 0, string.Format("Received CardCode: {0}", e.CardCode));
            }
            return false;
        }
        void m_paymentProvider_ActivationEvent(object sender, EventArgs e)
        {
            Status(eStatusType.Info, 0, "Pinpad requires activation, press code + OK on the pinpad!");

        }
        void Non_Visible_Buttons()
        {
            btnVuxen.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button3.Hide();
            button2.Hide();
            btnVuxen.Hide();
            familjBtnPrisLable.Hide();
            barnBtnPrisLable.Hide();
            vuxenBtnPrisLable.Hide();
        }

        void Visible_Buttons()
        {
            btnVuxen.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button3.Show();
            button2.Show();
            btnVuxen.Show();
            btNo.Enabled = true;
            btNo.Show();
            familjBtnPrisLable.Show();
            barnBtnPrisLable.Show();
            vuxenBtnPrisLable.Show();

        }
        //Fortsatt button
        private void button5_Click(object sender, EventArgs e)
        {
            if (guiState == GUI_STATE.TICKET_GUI)
            {
                if (totalprise == 0)
                {
                    MessageBox.Show("Please select minimun one ticket... ");
                    return;
                }
                else if (m_paymentProvider == null)
                {
                    MessageBox.Show("Connect to terminal first!");
                    return;
                }

                if (BeginPurchase())
                {
                    finishedCounter = 2;
                    Non_Visible_Buttons();
                    guiState = GUI_STATE.TERMINAL_GUI;

                }

            }
            else if (guiState == GUI_STATE.TERMINAL_GUI)
            {
                m_paymentProvider.AnswerPositive();
                //pbImage.Visible = true;
            }

        }

        //private void change_Btn_Color(object sender, MouseEventArgs e)
        //{

        //    btYes.BackColor = Color.Red;
        //    btNo.BackColor = Color.Red;

        //    vuxenBtnPrisLable.BackColor = Color.Red;
        //    btnVuxen.BackColor = Color.Red;

        //    barnBtnPrisLable.BackColor = Color.Red;
        //    button2.BackColor = Color.Red;

        //    familjBtnPrisLable.BackColor = Color.Red;
        //    button3.BackColor = Color.Red;

        //}


        /// <summary>
        /// Begin purchase
        /// </summary>
        /// <returns></returns>
        public bool BeginPurchase()
        {


            //Använd interface?
            m_paymentProvider.UseGUI = false;

            //Språk?
            if (transflags)
            {
                switch(transCode)
                {
                    case 44:
                        m_paymentProvider.Texts = englishTexts;
                        break;
                    case 46:
                        m_paymentProvider.Texts = swedishTexts;
                        break;
                    default:
                        m_paymentProvider.Texts = polishTexts;
                        break;
                }
                //textHandlingOnScreen();
            }


            //Skapa testprodukt
            Gordion.Payment.Products oProducts = new Products();


            //Momssatstest med olika momssatser

            if (noofBarn > 0)
            {
                oProducts.Add(new Product(0, 1, "Barn/Ungdom", "", noofBarn, barn, 0.25m));
            }
            if (noofVuxen > 0)
            {
                oProducts.Add(new Product(0, 2, "Vuxen", "", noofVuxen, vuxen, 0.25m));
            }
            if (noofFamilj > 0)
            {
                oProducts.Add(new Product(0, 3, "Duo/Familj", "", noofFamilj, familj, 0.25m));
            }


            //Börja betalning
            PaymentInit iRes = m_paymentProvider.BeginPayment(oProducts, "Biljett/Ticket/Bilet");

            if (iRes != PaymentInit.PAYINIT_OK)
            {

                MessageBox.Show("Init Error: " + m_paymentProvider.Data.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Text Handling Function on Kiosk Screen
        /// </summary>

        private void textHandlingOnScreen()
        {

            if (transCode == 44)
            {
                //English_Labels_Translation();                
            }

            else if (transCode == 46)
            {
                Swedish_Lables_Translation();
            }
            else if(transCode == 48)
            {
                Polish_Lables_Translation();
            }
            else
            {

            }

        }


        private void Polish_Lables_Translation()
        {
            PaymentTexts tranText = new PaymentTexts(transCode);
            PaymentTexts.cCard Card = new PaymentTexts.cCard();
            PaymentTexts.cPIN Pin = new PaymentTexts.cPIN();

            if (textFlag == 0)
            {
                tranText.Produce.Header = "Click on the Continue Button :";
                tranText.Produce.Row1 = "Bus ticket";
                tranText.Produce.Row2 = "Total amount to pay : " + totalprise + " SEK ";
                tranText.Produce.Row3 = "Amount will be deducted from your account.";

                lbHeader.Text = tranText.Produce.Header;
                lbRow1.Text = tranText.Produce.Row1;
                lbRow2.Text = tranText.Produce.Row2;
                lbRow3.Text = tranText.Produce.Row3;


                textFlag = 1;
            }

            else if (textFlag == 1)
            {

                Card.Header = "Insert Card :";
                pbOurs.Image = Properties.Resources.insertCardImage;
                pbCardTypeImage.Image = Properties.Resources.cardlogos_2;

                Card.Row1 = "Valid cards displayed at the bottom.";
                Card.Row2Insert = "Total amount to pay : " + totalprise + " SEK ";

                lbHeader.Text = Card.Header;
                lbRow1.Text = Card.Row1;
                lbRow2.Text = Card.Row2Insert;


                textFlag = 2;
            }

            else if (textFlag == 2)
            {
                Card.Header = "Remove your card..";
                pbOurs.Image = Properties.Resources.CardRemove;
                Card.Row3RemoveCard = "Amount will be Deducted from your credit card.";

                lbHeader.Text = Card.Header;
                lbRow3.Text = Card.Row3RemoveCard;

                textFlag = 3;
            }

            else if (textFlag == 3)
            {

                Pin.Header = "Enter your card PIN..";
                Pin.Row3 = "Press OK, on the keypad..";
                pbOurs.Image = Properties.Resources.keypad111;

                lbHeader.Text = Pin.Header;
                lbRow3.Text = Pin.Row3;

              
                lbHeader.Text = "Transaction Cancelled";
                m_paymentProvider.AnswerNegative();
                m_paymentProvider.CancelPayment();
                m_paymentProvider.Dispose();
                loadmaininterface();

                textFlag = 4;
            }


            else
            {
                textFlag = 0;
            }
        }

        private void Swedish_Lables_Translation()
        {
            PaymentTexts tranText = new PaymentTexts(transCode);
            PaymentTexts.cCard Card = new PaymentTexts.cCard();
            PaymentTexts.cPIN Pin = new PaymentTexts.cPIN();

            if (textFlag == 0)
            {
                tranText.Produce.Header = "Click on the Continue Button :";
                tranText.Produce.Row1 = "Bus ticket";
                tranText.Produce.Row2 = "Total amount to pay : " + totalprise + " SEK ";
                tranText.Produce.Row3 = "Amount will be deducted from your account.";

                lbHeader.Text = tranText.Produce.Header;
                lbRow1.Text = tranText.Produce.Row1;
                lbRow2.Text = tranText.Produce.Row2;
                lbRow3.Text = tranText.Produce.Row3;


                textFlag = 1;
            }

            else if (textFlag == 1)
            {

                Card.Header = "Insert Card :";
                pbOurs.Image = Properties.Resources.insertCardImage;
                pbCardTypeImage.Image = Properties.Resources.cardlogos_2;

                Card.Row1 = "Valid cards displayed at the bottom.";
                Card.Row2Insert = "Total amount to pay : " + totalprise + " SEK ";

                lbHeader.Text = Card.Header;
                lbRow1.Text = Card.Row1;
                lbRow2.Text = Card.Row2Insert;


                textFlag = 2;
            }

            else if (textFlag == 2)
            {
                Card.Header = "Remove your card..";
                pbOurs.Image = Properties.Resources.CardRemove;
                Card.Row3RemoveCard = "Amount will be Deducted from your credit card.";

                lbHeader.Text = Card.Header;
                lbRow3.Text = Card.Row3RemoveCard;

                textFlag = 3;
            }

            else if (textFlag == 3)
            {

                Pin.Header = "Enter your card PIN..";
                Pin.Row3 = "Press OK, on the keypad..";
                pbOurs.Image = Properties.Resources.keypad111;

                lbHeader.Text = Pin.Header;
                lbRow3.Text = Pin.Row3;

                //Cancel button eventHaldler 
                //cancellClickFlag 
                lbHeader.Text = "Transaction Cancelled";
                m_paymentProvider.AnswerNegative();
                m_paymentProvider.CancelPayment();
                m_paymentProvider.Dispose();
                loadmaininterface();

                textFlag = 4;
            }


            else
            {
                textFlag = 0;
            }
        }

        private void InitializeEnglishTranslation(PaymentTexts tranText)
        {
            PaymentTexts.cCard Card = tranText.Card;
            PaymentTexts.cPIN Pin = tranText.PIN;

            tranText.Start.Header = "Click on the Continue Button:";
            tranText.Start.Row1 = "Bus ticket";
            //tranText.Start.Row2 = "Total amount to pay : " + totalprise + " SEK ";
            tranText.Start.Row3 = "Amount will be deducted from your account.";

            Card.Header = "Card";
            Card.Row1 = "Valid cards displayed at the bottom.";
            Card.Row2Insert = "Insert Card :"; // "Total amount to pay : " + totalprise + " SEK ";
            Card.Row3RemoveCard = "Remove your card.."; // "Amount will be deducted from your account.";

            Pin.Header = "Enter your card PIN..";
            Pin.Row3 = "Press OK, on the keypad..";
        }

        private void ImageHandling()
        {
            if (paymentState == PaymentState.PS_CARD)
            {
                pbOurs.Visible = true;
                pbOurs.Image = Properties.Resources.insertCardImage;
                pbCardTypeImage.Image = Properties.Resources.cardlogos_2;
            }

            else if (paymentState == PaymentState.PS_TAKE_CARD)
            {
                pbOurs.Visible = true;
                pbOurs.Image = Properties.Resources.CardRemove;
            }

            else if (paymentState == PaymentState.PS_PIN)
            {
                pbOurs.Visible = true;
                pbOurs.Image = pbImage.Image;// Properties.Resources.keypad111;

            }
            else if(paymentState == PaymentState.PS_FINISHED)
            {
                pbOurs.Visible = false;
            }
            else
            {
                pbOurs.Visible = false;
            }
        }




        /// <summary>
        /// Buttons, Barn, Vuxen o Familj
        /// </summary>

        //No on Vuxen
        private void button1_Click(object sender, EventArgs e)
        {
            noofVuxen += 1;
            noofV.Text = "x " + noofVuxen.ToString();
            noofV_Total.Text = Convert.ToString(noofVuxen * vuxen);
            TotalPris.Text = Convert.ToString(totalSumma());
        }

        //No of Barn/Ungdom
        private void button2_Click(object sender, EventArgs e)
        {
            noofBarn += 1;
            noofBung.Text = "x " + noofBarn.ToString();
            noofB_Total.Text = Convert.ToString(noofBarn * barn);
            TotalPris.Text = Convert.ToString(totalSumma());
        }

        // No of Familj
        private void button3_Click(object sender, EventArgs e)
        {
            noofFamilj += 1;
            noofDF.Text = "x " + noofFamilj.ToString();
            noofDF_Total.Text = Convert.ToString(noofFamilj * familj);
            TotalPris.Text = Convert.ToString(totalSumma());
        }

        //Total Price Function
        public decimal totalSumma()
        {
            if (noofVuxen >= 1 || noofBarn >= 1 || noofFamilj >= 1)
            {
                return totalprise = (noofVuxen * vuxen) + (noofBarn * barn) + (noofFamilj * familj);
            }
            else
            {
                return ((noofVuxen * vuxen) + (noofBarn * barn));
            }

        }

        private void resetTickets()
        {
            noofVuxen = 0;              // int field
            noofV.Text = "0";           // Lable in the ticket 
            noofV_Total.Text = "0";     // Lable in the ticket

            noofBarn = 0;               // int field
            noofBung.Text = "0";        // Lable in the ticket
            noofB_Total.Text = "0";     // Lable in the ticket

            noofFamilj = 0;             // int field
            noofDF.Text = "0";          // Lable in the ticket
            noofDF_Total.Text = "0";    // Lable in the ticket

            totalprise = 0;             // int field
            TotalPris.Text = "0";       // Lable in the ticket
            btYes.Enabled = true;
            btYes.Visible = true;

        }

        //Avbryt button
        private void button4_Click(object sender, EventArgs e)
        {
            if (guiState == GUI_STATE.TERMINAL_GUI)
            {
                m_paymentProvider.AnswerNegative();
            }
            else
            {
                resetTickets();
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }


        //Translation ..........................................................
        private void getRes(CultureInfo ci)
        {
            Assembly a = Assembly.Load("WindowsFormsApplication1");
            ResourceManager rm = new ResourceManager("WindowsFormsApplication1.Lang.Langres", a);
            label1.Text = rm.GetString("label1", ci);
            label2.Text = rm.GetString("label2", ci);
            TicketTitle.Text = rm.GetString("TicketTitle", ci);
            groupBox1.Text = rm.GetString("groupBox1", ci);
            TotalPristLb.Text = rm.GetString("TotalPristLb", ci);
            familjBtnPrisLable.Text = rm.GetString("familjBtnPrisLable", ci);
            VuxentLb.Text = rm.GetString("VuxentLb", ci);
            familjtLb.Text = rm.GetString("familjtLb", ci);
            barntLb.Text = rm.GetString("barntLb", ci);
            btnVuxen.Text = rm.GetString("button1", ci);
            button2.Text = rm.GetString("button2", ci);
            button3.Text = rm.GetString("button3", ci);
            btNo.Text = rm.GetString("button4", ci);
            btYes.Text = rm.GetString("button5", ci);
        }

        private void MainPolishTranslation()
        {
            CultureInfo ci = new CultureInfo("pl-PL");
            getRes(ci);
        }
        private void MainSwedishTranslation()
        {
            CultureInfo ci = new CultureInfo("sv-SE");
            getRes(ci);
        }
        private void MainEnglishTranslation()
        {
            CultureInfo ci = new CultureInfo("en-US");
            getRes(ci);
        }
        private void button6Poland_Click(object sender, EventArgs e)
        {
            transflags = true;
            transCode = 48;
            MainPolishTranslation();
        }
        private void button7Swedish_Click(object sender, EventArgs e)
        {
            transflags = true;
            transCode = 46;
            MainSwedishTranslation();
        }
        private void button8English_Click(object sender, EventArgs e)
        {
            transflags = true;
            transCode = 44;
            MainEnglishTranslation();
        }
       
    }
}
