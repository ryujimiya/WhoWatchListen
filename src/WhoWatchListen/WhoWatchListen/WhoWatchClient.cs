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
            public int id { get; set; }
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
            public long started_at { get; set; }
            public int time_limit { get; set; }
            public int live_act_limit { get; set; }
            public int extension_option_limit { get; set; }
            public int total_view_count { get; set; }
            public int comment_count { get; set; }
            public int item_count { get; set; }
            public string live_finished_image_url { get; set; }
            public string latest_thumbnail_url { get; set; }
            public int running_time { get; set; }
            public bool is_mute { get; set; }
            public bool is_automatic_extension { get; set; }
            public int view_count { get; set; }
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
            public int count { get; set; }
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
        /// ライブID
        /// </summary>
        public string LiveId
        {
            get { return liveId;  }
        }

        /// <summary>
        /// ライブID
        /// </summary>
        private string liveId = "";
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
        /// ふわっちコメント取得タイマー
        /// </summary>
        private DispatcherTimer whoWatchGetCommentDTimer;

        private bool IsTimerProcess = false; 


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WhoWatchClient()
        {
            whoWatchGetCommentDTimer = new DispatcherTimer(DispatcherPriority.Normal);
            whoWatchGetCommentDTimer.Interval = new TimeSpan(0, 0, 2);
            whoWatchGetCommentDTimer.Tick += new EventHandler(whoWatchGetCommentDTimer_Tick);
        }

        /// <summary>
        /// 開始する
        /// </summary>
        /// <param name="liveId"></param>
        public void Start(string liveId)
        {
            // ライブIDをセットする
            this.liveId = liveId;

            // ふわっち形式の時間の生成
            this.lastUpdatedAt = MyUtil.GetUnixTime(DateTime.Now).ToString() + "000";
            System.Diagnostics.Debug.WriteLine("lastUpdatedAt (Initial):" + lastUpdatedAt);

            whoWatchGetCommentDTimer.Start();
        }

        /// <summary>
        /// 停止する
        /// </summary>
        public void Stop()
        {
            whoWatchGetCommentDTimer.Stop();
        }

        /// <summary>
        /// ふわっちコメント取得タイマーイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void whoWatchGetCommentDTimer_Tick(object sender, EventArgs e)
        {
            if (IsTimerProcess)
            {
                return;
            }

            IsTimerProcess = true;
            whoWatchGetCommentsHandle();
            IsTimerProcess = false;
        }

        /// <summary>
        /// ふわっちコメント取得処理
        /// </summary>
        private void whoWatchGetCommentsHandle()
        {
            string apiUrl = " https://api.whowatch.tv/lives/" + liveId
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
                    if (OnCommentReceiveEach != null)
                    {
                        OnCommentReceiveEach(this, tagtComment);
                    }

                }
                if (OnCommentReceiveDone != null)
                {
                    OnCommentReceiveDone(this);
                }


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
