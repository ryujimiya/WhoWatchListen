using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WhoWatchListen
{
    /// <summary>
    /// コメントデータ
    /// </summary>
    public class UiCommentData
    {
        public string UserThumbUrl { get; set; }
        public string UserName { get; set; }
        public string CommentStr { get; set; }

    }

    /// <summary>
    /// ビューモデル
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// コメントデータリストのデータバインディング用
        /// </summary>
        public ObservableCollection<UiCommentData> UiCommentDataCollection { get; set; }

        public ViewModel()
        {
            UiCommentDataCollection = new ObservableCollection<UiCommentData>();
            /*
            // dummy
            UiCommentData workUiCommentData;
            workUiCommentData= new UiCommentData();
            workUiCommentData.UserName = "匿名";
            workUiCommentData.CommentStr = "Hello, World!";
            workUiCommentData.UserThumbUrl = "";
            UiCommentDataCollection.Add(workUiCommentData);
            workUiCommentData = new UiCommentData();
            workUiCommentData.UserName = "匿名";
            workUiCommentData.CommentStr = "Hello, World!";
            workUiCommentData.UserThumbUrl = "";
            UiCommentDataCollection.Add(workUiCommentData);
            workUiCommentData = new UiCommentData();
            workUiCommentData.UserName = "匿名";
            workUiCommentData.CommentStr = "Hello, World!";
            workUiCommentData.UserThumbUrl = "";
            UiCommentDataCollection.Add(workUiCommentData);
            */
        }
    }
}
