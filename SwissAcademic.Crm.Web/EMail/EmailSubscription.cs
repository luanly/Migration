using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class EmailSubscription
    {
        #region Eigenschaften

        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> ListIdsInOtherLanguages { get; } = new List<string>();

        #endregion

        #region Statische Instanzen

        #region NewsEnglish

        static EmailSubscription _newsEnglish;
        public static EmailSubscription NewsEnglish
        {
            get
            {
                if (_newsEnglish == null)
                {
                    _newsEnglish = new EmailSubscription
                    {
                        Id = MailChimpListIds.NewsEnglish,
                        Name = "Citavi News",

                    };
                    _newsEnglish.ListIdsInOtherLanguages.Add(MailChimpListIds.NewsGerman);
                }
                return _newsEnglish;
            }
        }

        #endregion

        #region NewsGerman

        static EmailSubscription _newsGerman;
        public static EmailSubscription NewsGerman
        {
            get
            {
                if (_newsGerman == null)
                {
                    _newsGerman = new EmailSubscription
                    {
                        Id = MailChimpListIds.NewsGerman,
                        Name = "Citavi Nachrichten"
                    };
                    _newsGerman.ListIdsInOtherLanguages.Add(MailChimpListIds.NewsEnglish);
                }
                return _newsGerman;
            }
        }

        #endregion

        #endregion
    }
}
