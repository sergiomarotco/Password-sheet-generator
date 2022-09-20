namespace Marotco_generator
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Office.Interop.Word;
    using PasswordGenerator;

    /// <summary>
    /// Основная форма.
    /// </summary>
    public partial class Form1 : Form
    {
        private Microsoft.Office.Interop.Word.Application app;
        private Document doc;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// Основной класс.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Функция замены строк в word файле.
        /// </summary>
        /// <param name="str_old">Заменяемая страка.</param>
        /// <param name="str_new">Строка которой заменяют.</param>
        public void FindReplace(string str_old, string str_new)
        {
            object missingObject = null;
            object item = WdGoToItem.wdGoToPage;
            object whichItem = WdGoToDirection.wdGoToFirst;
            object replaceAll = WdReplace.wdReplaceAll;
            object forward = true;
            object matchAllWord = true;
            object matchCase = false;
            object originalText = str_old;
            object replaceText = str_new;

            doc.GoTo(ref item, ref whichItem, ref missingObject, ref missingObject);
            foreach (Range rng in doc.StoryRanges)
            {
                rng.Find.Execute(
                    ref originalText,
                    ref matchCase,
                    ref matchAllWord,
                    ref missingObject,
                    ref missingObject,
                    ref missingObject,
                    ref forward,
                    ref missingObject,
                    ref missingObject,
                    ref replaceText,
                    ref replaceAll,
                    ref missingObject,
                    ref missingObject,
                    ref missingObject,
                    ref missingObject);
            }

            /* Код работающий быстрее но не работающий с колонтитулами:
            Find find = app.Selection.Find;

            find.Text = str_old; // текст поиска
            find.Replacement.Text = str_new; // текст замены
            find.Execute(FindText: str_old, MatchCase: false, MatchWholeWord: true, MatchWildcards: false,
                        MatchSoundsLike: missing, MatchAllWordForms: false, Forward: true, Wrap: WdFindWrap.wdFindContinue,
                        Format: false, ReplaceWith: str_new, Replace: WdReplace.wdReplaceAll);
            object matchCase = true; object matchWholeWord = true;
            object matchWildCards = false; object matchSoundLike = false;
            object nmatchAllForms = false; object forward = true;
            object format = false; object matchKashida = false;
            object matchDiactitics = false; object matchAlefHamza = false;
            object matchControl = false; object read_only = false;
            object visible = true; object replace = 2;
            object wrap = 1;
            foreach (Range range in doc.StoryRanges)
            {
                if (range.StoryType == WdStoryType.wdEvenPagesHeaderStory)
                {
                    find.Execute(FindText: str_new, MatchCase: false, MatchWholeWord: false, MatchWildcards: false,
                    MatchSoundsLike: missing, MatchAllWordForms: false, Forward: true, Wrap: WdFindWrap.wdFindContinue,
                    Format: false, ReplaceWith: str_new, Replace: WdReplace.wdReplaceAll);
                }
            }*/
        }

        /// <summary>
        /// Сгенериратьлист с паролем.
        /// </summary>
        /// <param name="type">true - пароль на контейнер, false - на zip архив.</param>
        public void FileGenerate(bool type)
        {
            switch (type)
            {
                case true:
                    if (textBox_container.Text.Length == 0)
                    {
                        Gen_container_pass(true);
                    }

                    break;
                case false:
                    if (textBox_ZIP.Text.Length == 0)
                    {
                        Gen_container_pass(false);
                    }

                    break;
                default:
                    break;
            }

            MessageBox.Show("!После закрытия окна, все процессы winword.exe будут убиты без сохранения!");
            Stopwatch st = new Stopwatch();
            st.Start();
            foreach (var proc in Process.GetProcessesByName("winWord"))
            {
                proc.Kill();
            }

            // открываем в word документ
            app = new Microsoft.Office.Interop.Word.Application();

            // File.WriteAllBytes("Template.docx", Properties.Resources.Template.ToArray());
            doc = app.Documents.Open(Directory.GetCurrentDirectory().ToString() + @"\\Template.docx");
            int p = 0;
            char[] password = new char[0];
            switch (type)
            {
                case true:
                    password = textBox_container.Text.ToCharArray();
                    break;
                case false:
                    password = textBox_ZIP.Text.ToCharArray();
                    break;
                default:
                    break;
            }

            for (int i = 0; i < 18; i++)
            {
                if (i < trackBar1.Value)
                {
                    FindReplace("[" + i + "]", password[p].ToString()); // выполняем в тексте документа замену текста
                    p++;
                }
                else
                {
                    FindReplace("[" + i + "]", string.Empty); // выполняем в тексте документа замену текста
                }
            }

            switch (type)
            {
                case true:
                    FindReplace("[от]", "КОНТЕЙНЕРА"); // выполняем в тексте документа замену текста
                    Clipboard.SetData(DataFormats.Text, (object)textBox_container.Text); // копируем в буфер
                    break;
                case false:
                    FindReplace("[от]", "ZIP архива"); // выполняем в тексте документа замену текста
                    Clipboard.SetData(DataFormats.Text, (object)textBox_ZIP.Text); // копируем в буфер
                    break;
                default:
                    break;
            }

            SaveCloseFile(true); // закрываем открытый в word документ File.Delete(Directory.GetCurrentDirectory().ToString() + @"\\Template.docx");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FileGenerate(true);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            FileGenerate(false);
        }

        /// <summary>
        /// Путь к файлу с паролем от контейнера.
        /// </summary>
        private string fileContainer = string.Empty;

        /// <summary>
        /// Путь к файлу с паролем от ZIP архива.
        /// </summary>
        private string fileContainerZIP = string.Empty;

        /// <summary>
        /// Закрытие Word и открытие файла.
        /// </summary>
        /// <param name="type">true - пароль на контейнер, false - на zip архив.</param>
        public void SaveCloseFile(bool type)
        {
            DateTime d = DateTime.Now;
            string cD = Directory.GetCurrentDirectory().ToString();
            string time2 = d.Year.ToString() + "." + d.Month + "." + d.Day + " " + d.Hour + "-" + d.Minute;
            switch (type)
            {
                case true:
                    fileContainer = cD + @"\\" + time2 + " Пароль_на_контейнер.docx"; // новый файл на основе файла-шаблона
                    app.ActiveDocument.SaveAs(fileContainer);
                    app.ActiveDocument.Close();
                    app.Quit();
                    app = null;
                    Process.Start(fileContainer);
                    break;
                case false:
                    fileContainerZIP = cD + @"\\" + time2 + " Пароль_на_архив.docx"; // новый файл на основе файла-шаблона;
                    app.ActiveDocument.SaveAs(fileContainerZIP);
                    app.ActiveDocument.Close();
                    app.Quit();
                    app = null;
                    Process.Start(fileContainerZIP);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Сгенерировать пароль и записать в textbox.
        /// </summary>
        /// <param name="type">true - пароль на контейнер, false - на zip архив.</param>
        private void Gen_container_pass(bool type)
        {
            var pwd = new Password(Convert.ToInt32(textBox_pas_maxL.Text)); // настраиваем генератор пароля
            string result = pwd.Next();                             // генерируем пароль
            result = result.Replace('i', '*'); // замена плохочитаемых символов
            result = result.Replace('I', '?');
            result = result.Replace('L', '#');
            result = result.Replace('|', '(');
            result = result.Replace('o', '@');
            result = result.Replace('O', '!');
            char[] resault_char = result.ToArray();
            for (int i = 4; i < resault_char.Length; i += 5)
            {
                resault_char[i] = '-';
            }

            result = new string(resault_char);
            switch (type)
            {// записываем пароль
                case true:
                    textBox_container.Text = result;
                    break;
                case false:
                    textBox_ZIP.Text = result;
                    break;
                default:
                    break;
            }

            Clipboard.SetData(DataFormats.Text, (object)result); // копируем в буфер
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Gen_container_pass(true);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Gen_container_pass(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.logo;
            textBox_pas_maxL.Text = trackBar1.Value.ToString();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            textBox_pas_maxL.Text = trackBar1.Value.ToString();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/sergiomarotco/");
        }
    }
}