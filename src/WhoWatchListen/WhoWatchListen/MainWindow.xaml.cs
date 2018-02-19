using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        private string TitleBase = "";
        /// <summary>
        /// 棒読みちゃん
        /// </summary>
        private MyUtilLib.BouyomiChan BouyomiChan = new MyUtilLib.BouyomiChan();

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
            TitleBase = this.Title + " " + MyUtil.GetFileVersion();
            this.Title = TitleBase;
        }

        /// <summary>
        /// ウィンドウが開かれた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            whoWatchClient = new WhoWatchClient();
            whoWatchClient.OnCommentReceiveEach += WhoWatchClient_OnCommentReceiveEach;
            whoWatchClient.OnCommentReceiveDone += WhoWatchClient_OnCommentReceiveDone;
        }


        /// <summary>
        /// ウィンドウが閉じられようとしている
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            whoWatchClient.Stop();
            BouyomiChan.ClearText();
            BouyomiChan.Dispose();
        }

        /// <summary>
        /// ふわっちクライアントからコメントを受信した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="comment"></param>
        private void WhoWatchClient_OnCommentReceiveEach(WhoWatchClient sender, CommentStruct comment)
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
            BouyomiChan.Talk(uiCommentData.CommentStr);
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
        private void WhoWatchClient_OnCommentReceiveDone(WhoWatchClient sender)
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
            string liveId = whoWatchClient.LiveId;
            if (liveId == "")
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
            updateLiveId();
        }

        /// <summary>
        /// ライブIDの更新
        /// </summary>
        private void updateLiveId()
        {
            // いま稼働しているクライアントを停止する
            whoWatchClient.Stop();

            BouyomiChan.ClearText();

            // 新しいライブIdを取得
            string liveId = liveIdTextBox.Text;

            // クライアントを開始する
            whoWatchClient.Start(liveId);

        }

        /// <summary>
        /// ライブIDテキストボックスのキーアップイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void liveIdTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                updateLiveId();
            }
        }
    }

}
