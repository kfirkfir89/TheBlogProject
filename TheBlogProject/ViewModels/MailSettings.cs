﻿namespace TheBlogProject.ViewModels
{
    public class MailSettings
    {
        //so we can configure and use and smtp server
        //from google for example

        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
