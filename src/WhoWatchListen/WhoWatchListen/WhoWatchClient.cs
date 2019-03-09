using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading; // DispatcherTimer
using System.Net; // WebClient
using System.IO; // StreamReader
using Newtonsoft.Json;
using MyUtilLib;

namespace WhoWatchListen
{    /// <summary>
     /// コメント受信イベントハンドラデリゲート
     /// </summary>
     /// <param name="sender">チャットクライアント</param>
     /// <param name="comment">受信したコメント構造体</param>
    public delegate void OnCommentReceiveEachDelegate(WhoWatchClient sender, CommentStruct comment);
    /// <summary>
    /// コメント受信完了イベントハンドラデリゲート
    /// <param name="sender">チャットクライアント</param>
    /// </summary>
    public delegate void OnCommentReceiveDoneDelegate(WhoWatchClient sender);
    /// <summary>
    /// ライブIDが変更されたときのイベントハンドラデリゲート
    /// </summary>
    /// <param name="sender">チャットクライアント</param>
    public delegate void OnLiveIdChangedDelegate(WhoWatchClient sender);


    /// <summary>
    /// コメント構造体
    /// </summary>
    public struct CommentStruct
    {
        /// <summary>
        /// コメントID
        /// </summary>
        //public uint Id;
        public ulong Id;
        /// <summary>
        /// コメントテキスト
        /// </summary>
        public string Text;
        /// <summary>
        /// ユーザー名
        /// </summary>
        public string UserName;
        /// <summary>
        /// 時刻
        /// </summary>
        public string TimeStr;
        /// <summary>
        /// ユーザーサムネールURL
        /// </summary>
        public string UserThumbUrl;
        /// <summary>
        /// 棒読みちゃんの音を出す？
        /// </summary>
        public bool IsBouyomiOn;
    }

    public class WhoWatchClient
    {
        ///////////////////////////////////////////////////////////////////////
        // 型
        ///////////////////////////////////////////////////////////////////////
        //------------------------------------------------------------------
        // プロフィール取得API用
        public class Live2
        {
            public ulong id { get; set; }
            public string title { get; set; }
            public string client_type { get; set; }
            public long started_at { get; set; }
            public bool hide_category_default_badge { get; set; }
            public string thumbnail_url { get; set; }
            public ulong running_time { get; set; }
            public bool is_comment_disallowed { get; set; }
        }

        public class WhoWatchProfileApiObject
        {
            public ulong user_id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string icon_url { get; set; }
            public string background_url { get; set; }
            public ulong show_follow_list { get; set; }
            public ulong show_follower_list { get; set; }
            public Live2 live { get; set; }
            public string account_name { get; set; }
            public string user_path { get; set; }
            public ulong follow_count { get; set; }
            public ulong follower_count { get; set; }
            public bool is_follow { get; set; }
            public string twitter_id { get; set; }
            public bool is_blocked { get; set; }
            public string gender { get; set; }
            public string date_of_birth { get; set; }
            public string area { get; set; }
            public List<string> likes { get; set; }
            public bool is_follow_backed { get; set; }
            public ulong live_history_count { get; set; }
            public bool is_push_registered { get; set; }
        }

        //------------------------------------------------------------------
        // コメント取得API用
        public class UserProfile
        {
            public bool is_date_of_birth_today { get; set; }
        }

        public class User
        {
            public ulong id { get; set; }
            public string user_type { get; set; }
            public UserProfile user_profile { get; set; }
            public string account_name { get; set; }
            public string user_path { get; set; }
            public string icon_url { get; set; }
            public string name { get; set; }
            public bool is_follow { get; set; }
            public bool is_follow_backed { get; set; }
            public bool is_push_registered { get; set; }
        }

        public class Category
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string badge { get; set; }
            public bool is_movie_only { get; set; }
            public bool is_radio_only { get; set; }
            public bool is_collaboration_ban { get; set; }
        }

        public class Live
        {
            public ulong id { get; set; }
            public string live_type { get; set; }
            public string title { get; set; }
            public string telop { get; set; }
            public string live_status { get; set; }
            public User user { get; set; }
            public string client_type { get; set; }
            public Category category { get; set; }
            public string substitute_image_url { get; set; }
            public ulong started_at { get; set; }
            public ulong time_limit { get; set; }
            public ulong live_act_limit { get; set; }
            public ulong extension_option_limit { get; set; }
            public ulong total_view_count { get; set; }
            public ulong comment_count { get; set; }
            public ulong item_count { get; set; }
            public string live_finished_image_url { get; set; }
            public string latest_thumbnail_url { get; set; }
            public ulong running_time { get; set; }
            public bool is_mute { get; set; }
            public bool is_automatic_extension { get; set; }
            public ulong view_count { get; set; }
        }

        public class UserProfile2
        {
            public bool is_date_of_birth_today { get; set; }
        }

        public class User2
        {
            public ulong id { get; set; }
            public string user_type { get; set; }
            public UserProfile2 user_profile { get; set; }
            public string account_name { get; set; }
            public string user_path { get; set; }
            public string icon_url { get; set; }
            public string name { get; set; }
        }

        public class Comment
        {
            public ulong id { get; set; }
            public User2 user { get; set; }
            public string comment_type { get; set; }
            public bool not_escaped { get; set; }
            public bool anonymized { get; set; }
            public string posted_at { get; set; }
            public string message { get; set; }
            public string escaped_message { get; set; }
            public bool is_silent_comment { get; set; }
        }

        public class LiveSentItem
        {
            public string name { get; set; }
            public string image_url { get; set; }
            public ulong count { get; set; }
        }

        public class WhoWatchApiObject
        {
            public Live live { get; set; }
            public List<Comment> comments { get; set; }
            public List<object> deleted_comment_ids { get; set; }
            public string updated_at { get; set; }
            public List<LiveSentItem> live_sent_items { get; set; }
            public bool is_item_updated { get; set; }
        }

        //////////////////////////////////////////
        /// <summary>
        /// ユーザーアカウント名
        /// </summary>
        public string AccountName { get; private set; }

        /// <summary>
        /// ライブID
        /// </summary>
        public ulong LiveId { get; private set; }

        /// <summary>
        /// 最終更新日時
        /// </summary>
        private string lastUpdatedAt = "0";
        /// <summary>
        /// コメント受信イベントハンドラ
        /// </summary>
        public event OnCommentReceiveEachDelegate OnCommentReceiveEach = null;
        /// <summary>
        /// コメント受信完了イベントハンドラ
        /// </summary>
        public event OnCommentReceiveDoneDelegate OnCommentReceiveDone = null;
        /// <summary>
        /// ライブIDが変更された時のイベントハンドラ
        /// </summary>
        public event OnLiveIdChangedDelegate OnLiveIdChanged = null;
        /// <summary>
        /// ふわっちコメント取得タイマー
        /// </summary>
        private DispatcherTimer whoWatchGetCommentDTimer;
        /// <summary>
        /// プロフィール取得タイマー
        /// </summary>
        private DispatcherTimer whoWatchGetProfileDTimer;

        private bool isTimerProcess = false; 

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WhoWatchClient()
        {
            whoWatchGetCommentDTimer = new DispatcherTimer(DispatcherPriority.Normal);
            whoWatchGetCommentDTimer.Interval = new TimeSpan(0, 0, 2);
            whoWatchGetCommentDTimer.Tick += new EventHandler(whoWatchGetCommentDTimer_Tick);
            whoWatchGetProfileDTimer = new DispatcherTimer(DispatcherPriority.Normal);
            whoWatchGetProfileDTimer.Interval = new TimeSpan(0, 0, 15);
            whoWatchGetProfileDTimer.Tick += new EventHandler(whoWatchGetProfileDTimer_Tick);
        }

        /// <summary>
        /// 開始する
        /// </summary>
        /// <param name="accountName"></param>
        public void Start(string accountName)
        {
            this.AccountName = accountName;
            this.LiveId = 0;

            whoWatchGetProfile();

            // ふわっち形式の時間の生成
            this.lastUpdatedAt = MyUtil.GetUnixTime(DateTime.Now).ToString() + "000";
            System.Diagnostics.Debug.WriteLine("lastUpdatedAt (Initial):" + lastUpdatedAt);

            whoWatchGetComments();

            whoWatchGetCommentDTimer.Start();
            whoWatchGetProfileDTimer.Start();
        }

        /// <summary>
        /// 停止する
        /// </summary>
        public void Stop()
        {
            whoWatchGetProfileDTimer.Stop();
            whoWatchGetCommentDTimer.Stop();
        }

        /// <summary>
        /// ふわっちプロフィール取得タイマーイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void whoWatchGetProfileDTimer_Tick(object sender, EventArgs e)
        {
            ulong prevLiveId = this.LiveId; // 前のライブIDを退避

            whoWatchGetProfile();

            if (prevLiveId != this.LiveId)
            {
                OnLiveIdChanged(this);
            }
        }

        /// <summary>
        /// ふわっちコメント取得タイマーイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void whoWatchGetCommentDTimer_Tick(object sender, EventArgs e)
        {
            if (isTimerProcess)
            {
                return;
            }

            isTimerProcess = true;
            whoWatchGetComments();
            isTimerProcess = false;
        }

        /// <summary>
        /// ふわっちプロフィール取得
        /// </summary>
        private void whoWatchGetProfile()
        {
            if (this.AccountName == "")
            {
                System.Diagnostics.Debug.WriteLine("[ERROR]whoWatchGetCommentsHandle AccountName = (empty)");
                return;
            }
            string apiUrl = " https://api.whowatch.tv/users/" + this.AccountName
                            + "/profile?polling=true";

            string recvStr = doHttpRequest(apiUrl);
            try
            {
                // JSON形式からふわっちAPIオブジェクトに変換
                WhoWatchProfileApiObject whoWatchApiObj = JsonConvert.DeserializeObject<WhoWatchProfileApiObject>(recvStr);
                if (whoWatchApiObj.live != null)
                {
                    // JSONオブジェクトからライブIDを取得
                    this.LiveId = whoWatchApiObj.live.id;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        /// <summary>
        /// ふわっちコメント取得処理
        /// </summary>
        private void whoWatchGetComments()
        {
            if (this.LiveId == 0)
            {
                //System.Diagnostics.Debug.WriteLine("[ERROR]whoWatchGetCommentsHandle liveId = 0");
                return;
            }
            string apiUrl = " https://api.whowatch.tv/lives/" + this.LiveId
                            + "?last_updated_at=" + lastUpdatedAt;

            string recvStr = doHttpRequest(apiUrl);

            try
            {
                // JSON形式からふわっちAPIオブジェクトに変換
                WhoWatchApiObject whoWatchApiObj = JsonConvert.DeserializeObject<WhoWatchApiObject>(recvStr);
                // JSONオブジェクトからコメント一覧を取得
                IList<CommentStruct> workCommentList = parseCommentsFromWhoWatchApiObj(whoWatchApiObj);

                foreach(CommentStruct tagtComment in workCommentList)
                {
                    OnCommentReceiveEach(this, tagtComment);

                }
                OnCommentReceiveDone(this);


                // 更新日時の更新
                lastUpdatedAt = whoWatchApiObj.updated_at;
                System.Diagnostics.Debug.WriteLine("lastUpdatedAt " + lastUpdatedAt);

            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        /// <summary>
        /// ライブIDからアカウント名を取得する
        /// </summary>
        /// <param name="liveId"></param>
        /// <returns></returns>
        public string GetAccountNameFromLiveId(ulong liveId)
        {
            string accountName = "";
            string lastUpdatedAt = MyUtil.GetUnixTime(DateTime.Now).ToString() + "000";
            string apiUrl = " https://api.whowatch.tv/lives/" + liveId
                            + "?last_updated_at=" + lastUpdatedAt;

            string recvStr = doHttpRequest(apiUrl);

            try
            {
                // JSON形式からふわっちAPIオブジェクトに変換
                WhoWatchApiObject whoWatchApiObj = JsonConvert.DeserializeObject<WhoWatchApiObject>(recvStr);
                if (whoWatchApiObj.live != null &&
                    whoWatchApiObj.live.user != null)
                {
                    // アカウント名(Note:ふわっちAPIではuser_pathにあたる. account_nameは別物)
                    accountName = whoWatchApiObj.live.user.user_path;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            return accountName;
        }

        /// <summary>
        /// HTTPリクエストを送信する
        /// </summary>
        /// <param name="url"></param>
        /// <returns>null:接続エラー または、recvStr:受信文字列</returns>
        private static string doHttpRequest(string url)
        {
            string recvStr = null;
            using (WebClient webClient = new WebClient())
            {
                Stream stream = null;
                try
                {
                    stream = webClient.OpenRead(url);
                }
                catch (Exception exception)
                {
                    // 接続エラー
                    System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                    return recvStr;
                }
                StreamReader sr = new StreamReader(stream);
                recvStr = sr.ReadToEnd();
            }
            return recvStr;
        }

        /// <summary>
        /// ふわっちAPIオブジェクトを解析してコメントリストを取得する
        /// </summary>
        /// <param name="whoWatchApiObj"></param>
        /// <returns></returns>
        IList<CommentStruct> parseCommentsFromWhoWatchApiObj(WhoWatchApiObject whoWatchApiObj)
        {
            IList<CommentStruct> workCommentList = new List<CommentStruct>();

            foreach (Comment comment in whoWatchApiObj.comments)
            {
                CommentStruct workComment = new CommentStruct();
                workComment.Id = comment.id;
                workComment.UserThumbUrl = comment.user.icon_url;
                workComment.UserName = comment.user.name;
                workComment.TimeStr = comment.posted_at;
                workComment.Text = comment.message;
                workComment.IsBouyomiOn = true; // 初期値

                //System.Diagnostics.Debug.WriteLine("Id " + workComment.Id);
                //System.Diagnostics.Debug.WriteLine("UserThumbUrl " + workComment.UserThumbUrl);
                //System.Diagnostics.Debug.WriteLine("UserName " + workComment.UserName);
                //System.Diagnostics.Debug.WriteLine("TimeStr " + workComment.TimeStr);
                //System.Diagnostics.Debug.WriteLine("Text " + workComment.Text);

                workCommentList.Add(workComment);
            }
            return workCommentList;

        }

    }
}
