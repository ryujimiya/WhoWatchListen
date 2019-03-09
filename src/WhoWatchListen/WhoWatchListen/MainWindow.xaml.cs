using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyUtilLib;

namespace WhoWatchListen
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// タイトルのベース
        /// </summary>
        private string titleBase = "";
        /// <summary>
        /// 棒読みちゃん
        /// </summary>
        private MyUtilLib.BouyomiChan bouyomiChan = new MyUtilLib.BouyomiChan();

        /// <summary>
        /// ふわっちクライアント
        /// </summary>
        private WhoWatchClient whoWatchClient;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // GUI初期処理
            titleBase = this.Title + " " + MyUtil.GetFileVersion();
            this.Title = titleBase;
        }

        /// <summary>
        /// ウィンドウが開かれた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            whoWatchClient = new WhoWatchClient();
            whoWatchClient.OnCommentReceiveEach += whoWatchClient_OnCommentReceiveEach;
            whoWatchClient.OnCommentReceiveDone += whoWatchClient_OnCommentReceiveDone;
            whoWatchClient.OnLiveIdChanged += WhoWatchClient_OnLiveIdChanged;
        }

        /// <summary>
        /// ウィンドウが閉じられようとしている
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            whoWatchClient.Stop();
            bouyomiChan.ClearText();
            bouyomiChan.Dispose();
        }

        /// <summary>
        /// ウィンドウのサイズが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ウィンドウの高さ Note:最大化のときthis.Heightだと値がセットされない
            double height = this.RenderSize.Height;
            // データグリッドの高さ変更
            stackPanel1.Height = height - SystemParameters.CaptionHeight;
            dataGrid.Height = stackPanel1.Height - wrapPanel1.Height;
        }

        /// <summary>
        /// ライブIDが変更された
        /// </summary>
        /// <param name="sender"></param>
        private void WhoWatchClient_OnLiveIdChanged(WhoWatchClient sender)
        {
            updateAccountName();
        }

        /// <summary>
        /// ふわっちクライアントからコメントを受信した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="comment"></param>
        private void whoWatchClient_OnCommentReceiveEach(WhoWatchClient sender, CommentStruct comment)
        {
            // コメントの追加
            UiCommentData uiCommentData = new UiCommentData();
            uiCommentData.UserThumbUrl = comment.UserThumbUrl;
            uiCommentData.UserName = comment.UserName;
            uiCommentData.CommentStr = comment.Text;

            System.Diagnostics.Debug.WriteLine("UserThumbUrl " + uiCommentData.UserThumbUrl);
            System.Diagnostics.Debug.WriteLine("UserName " + uiCommentData.UserName);
            System.Diagnostics.Debug.WriteLine("CommentStr " + uiCommentData.CommentStr);

            ViewModel viewModel = this.DataContext as ViewModel;
            ObservableCollection<UiCommentData> uiCommentDataList = viewModel.UiCommentDataCollection;
            uiCommentDataList.Add(uiCommentData);

            // コメントログを記録
            writeLog(uiCommentData.UserName, uiCommentData.CommentStr);

            // 棒読みちゃんへ送信
            bouyomiChan.Talk(uiCommentData.CommentStr);
        }

        /// <summary>
        /// コメントログを記録する
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="commentText"></param>
        private void writeLog(string userName, string commentText)
        {
            string logText = userName + "\t" + commentText;
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                @"comment.txt",
                true, // append : true
                System.Text.Encoding.GetEncoding("UTF-8"));
            sw.WriteLine(logText);
            sw.Close();
        }

        /// <summary>
        /// ふわっちクライアントのコメント受信が完了した
        /// </summary>
        /// <param name="sender"></param>
        private void whoWatchClient_OnCommentReceiveDone(WhoWatchClient sender)
        {
            // データグリッドを自動スクロール
            dataGridScrollToEnd();
        }

        /// <summary>
        /// データグリッドを自動スクロール
        /// </summary>
        private void dataGridScrollToEnd()
        {
            if (dataGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(dataGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        /// <summary>
        /// ふわっちボタンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void whoWatchBtn_Click(object sender, RoutedEventArgs e)
        {
            ulong liveId = whoWatchClient.LiveId;
            if (liveId == 0)
            {
                return;
            }

            string liveUrl = "https://whowatch.tv/viewer/" + whoWatchClient.LiveId;
            // ブラウザで開く
            System.Diagnostics.Process.Start(liveUrl);
        }

        /// <summary>
        /// 更新ボタンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            updateAccountName();
        }

        /// <summary>
        /// アカウント名の更新
        /// </summary>
        private void updateAccountName()
        {
            // いま稼働しているクライアントを停止する
            whoWatchClient.Stop();

            bouyomiChan.ClearText();

            // 新しいアカウント名を取得
            string accountName = accountNameTextBox.Text;

            if (Regex.IsMatch(accountName, "^[0-9]+$"))
            {
                // ライブIDが入力されたとき
                ulong liveId = ulong.Parse(accountName);
                // アカウント名を取得
                accountName = whoWatchClient.GetAccountNameFromLiveId(liveId);
                this.accountNameTextBox.Text = accountName;
            }

            // クライアントを開始する
            whoWatchClient.Start(accountName);
        }

        /// <summary>
        /// アカウント名テキストボックスのキーアップイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void accountNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                updateAccountName();
            }
        }
    }

}
