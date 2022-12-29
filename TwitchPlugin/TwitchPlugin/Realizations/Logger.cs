namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Text.RegularExpressions;

    public class Logger : ILogger
    {
        internal class MatchNoReplacer
        {
            private Int32 nMatch = 0;
            // Replace each Regex cc match with the number of the occurrence.
            public string ReplaceLogParam(Match m)
            {
                return "{"+$"{this.nMatch++}"+"}";
            }
        }

        // Updates all occurences of {word} in a string to the {0}, {1} etc so that String.Format will take it 
        // The former string is working for Microsoft Logging backed or custom loggers but not for String. 
        private String UpdateToPositionalParameters(String source)
        {
            Regex param = new Regex(@"{\w*\}");
            MatchNoReplacer replacer = new MatchNoReplacer();
            return param.Replace(source, new MatchEvaluator(replacer.ReplaceLogParam));
        }

        public void LogInformation(String message, params Object[] args)
        {
            TwitchPlugin.PluginLog.Info(String.Format(UpdateToPositionalParameters(message), args));
        }

        public void LogError(String message, params Object[] args)
        {
            TwitchPlugin.PluginLog.Error(String.Format(UpdateToPositionalParameters(message), args));
        }
    }
}