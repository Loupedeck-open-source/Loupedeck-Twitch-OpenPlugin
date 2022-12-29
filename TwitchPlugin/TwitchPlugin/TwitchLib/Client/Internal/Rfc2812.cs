namespace TwitchLib.Client.Internal
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>Class detailing Rfc2812 specifications</summary>
    public sealed class Rfc2812
    {
        // nickname   =  ( letter / special ) *8( letter / digit / special / "-" )
        // letter     =  %x41-5A / %x61-7A       ; A-Z / a-z
        // digit      =  %x30-39                 ; 0-9
        // special    =  %x5B-60 / %x7B-7D
        //                  ; "[", "]", "\", "`", "_", "^", "{", "|", "}"
        private static readonly Regex NicknameRegex = new Regex(@"^[A-Za-z\[\]\\`_^{|}][A-Za-z0-9\[\]\\`_\-^{|}]+$", RegexOptions.Compiled);

        private Rfc2812()
        {
        }

        /// <summary>
        /// Checks if the passed nickname is valid according to the RFC
        ///
        /// Use with caution, many IRC servers are not conform with this!
        /// </summary>
        public static Boolean IsValidNickname(String nickname)
        {
            return !String.IsNullOrEmpty(nickname) &&
                   NicknameRegex.Match(nickname).Success;
        }

        /// <summary>Pass message.</summary>
        public static String Pass(String password)
        {
            return $"PASS {password}";
        }

        /// <summary>Nick message.</summary>
        public static String Nick(String nickname)
        {
            return $"NICK {nickname}";
        }

        /// <summary>User message.</summary>
        public static String User(String username, Int32 usermode, String realname)
        {
            return $"USER {username} {usermode} * :{realname}";
        }

        /// <summary>Oper message.</summary>
        public static String Oper(String name, String password)
        {
            return $"OPER {name} {password}";
        }

        /// <summary>Privmsg message.</summary>
        public static String Privmsg(String destination, String message)
        {
            return $"PRIVMSG {destination} :{message}";
        }

        /// <summary>Notice message.</summary>
        public static String Notice(String destination, String message)
        {
            return $"NOTICE {destination} :{message}";
        }

        /// <summary>Join message.</summary>
        public static String Join(String channel)
        {
            return $"JOIN {channel}";
        }

        /// <summary>Join message.</summary>
        public static String Join(String[] channels)
        {
            return $"JOIN {String.Join(",", channels)}";
        }

        /// <summary>Join message.</summary>
        public static String Join(String channel, String key)
        {
            return $"JOIN {channel} {key}";
        }

        /// <summary>Join message.</summary>
        public static String Join(String[] channels, String[] keys)
        {
            return $"JOIN {String.Join(",", channels)} {String.Join(",", keys)}";
        }

        /// <summary>Part message.</summary>
        public static String Part(String channel)
        {
            return $"PART {channel}";
        }

        /// <summary>Part message.</summary>
        public static String Part(String[] channels)
        {
            return $"PART {String.Join(",", channels)}";
        }

        /// <summary>Part message.</summary>
        public static String Part(String channel, String partmessage)
        {
            return $"PART {channel} :{partmessage}";
        }

        /// <summary>Part message.</summary>
        public static String Part(String[] channels, String partmessage)
        {
            return $"PART {String.Join(",", channels)} :{partmessage}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String channel, String nickname)
        {
            return $"KICK {channel} {nickname}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String channel, String nickname, String comment)
        {
            return $"KICK {channel} {nickname} :{comment}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String[] channels, String nickname)
        {
            return $"KICK {String.Join(",", channels)} {nickname}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String[] channels, String nickname, String comment)
        {
            return $"KICK {String.Join(",", channels)} {nickname} :{comment}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String channel, String[] nicknames)
        {
            return $"KICK {channel} {String.Join(",", nicknames)}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String channel, String[] nicknames, String comment)
        {
            return $"KICK {channel} {String.Join(",", nicknames)} :{comment}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String[] channels, String[] nicknames)
        {
            return $"KICK {String.Join(",", channels)} {String.Join(",", nicknames)}";
        }

        /// <summary>Kick message.</summary>
        public static String Kick(String[] channels, String[] nicknames, String comment)
        {
            return $"KICK {String.Join(",", channels)} {String.Join(",", nicknames)} :{comment}";
        }

        /// <summary>Motd message.</summary>
        public static String Motd()
        {
            return "MOTD";
        }

        /// <summary>Motd message.</summary>
        public static String Motd(String target)
        {
            return $"MOTD {target}";
        }

        /// <summary>Luser message.</summary>
        public static String Lusers()
        {
            return "LUSERS";
        }

        /// <summary>Luser message.</summary>
        public static String Lusers(String mask)
        {
            return $"LUSER {mask}";
        }

        /// <summary>Lusers</summary>
        public static String Lusers(String mask, String target)
        {
            return $"LUSER {mask} {target}";
        }

        /// <summary>Version message.</summary>
        public static String Version()
        {
            return "VERSION";
        }

        /// <summary>Version message.</summary>
        public static String Version(String target)
        {
            return $"VERSION {target}";
        }

        /// <summary>Stats message.</summary>
        public static String Stats()
        {
            return "STATS";
        }

        /// <summary>Stats message.</summary>
        public static String Stats(String query)
        {
            return $"STATS {query}";
        }

        /// <summary>Stats message.</summary>
        public static String Stats(String query, String target)
        {
            return $"STATS {query} {target}";
        }

        /// <summary>Links message.</summary>
        public static String Links()
        {
            return "LINKS";
        }

        /// <summary>Links message.</summary>
        public static String Links(String servermask)
        {
            return $"LINKS {servermask}";
        }

        /// <summary>Links message.</summary>
        public static String Links(String remoteserver, String servermask)
        {
            return $"LINKS {remoteserver} {servermask}";
        }

        /// <summary>Time message.</summary>
        public static String Time()
        {
            return "TIME";
        }

        /// <summary>Time message.</summary>
        public static String Time(String target)
        {
            return $"TIME {target}";
        }

        /// <summary>Connect message.</summary>
        public static String Connect(String targetserver, String port)
        {
            return $"CONNECT {targetserver} {port}";
        }

        /// <summary>Connect message.</summary>
        public static String Connect(String targetserver, String port, String remoteserver)
        {
            return $"CONNECT {targetserver} {port} {remoteserver}";
        }

        /// <summary>Trace message.</summary>
        public static String Trace()
        {
            return "TRACE";
        }

        /// <summary>Trace message.</summary>
        public static String Trace(String target)
        {
            return $"TRACE {target}";
        }

        /// <summary>Admin message.</summary>
        public static String Admin()
        {
            return "ADMIN";
        }

        /// <summary>Admin message.</summary>
        public static String Admin(String target)
        {
            return $"ADMIN {target}";
        }

        /// <summary>Info message.</summary>
        public static String Info()
        {
            return "INFO";
        }

        /// <summary>Info message.</summary>
        public static String Info(String target)
        {
            return $"INFO {target}";
        }

        /// <summary>Servlist message.</summary>
        public static String Servlist()
        {
            return "SERVLIST";
        }

        /// <summary>Servlist message.</summary>
        public static String Servlist(String mask)
        {
            return $"SERVLIST {mask}";
        }

        /// <summary>Servlist message.</summary>
        public static String Servlist(String mask, String type)
        {
            return $"SERVLIST {mask} {type}";
        }

        /// <summary>Squery message.</summary>
        public static String Squery(String servicename, String servicetext)
        {
            return $"SQUERY {servicename} :{servicename}";
        }

        /// <summary>List message.</summary>
        public static String List()
        {
            return "LIST";
        }

        /// <summary>List message.</summary>
        public static String List(String channel)
        {
            return $"LIST {channel}";
        }

        /// <summary>List message.</summary>
        public static String List(String[] channels)
        {
            return $"LIST {String.Join(",", channels)}";
        }

        /// <summary>List message.</summary>
        public static String List(String channel, String target)
        {
            return $"LIST {channel} {target}";
        }

        /// <summary>List message.</summary>
        public static String List(String[] channels, String target)
        {
            return $"LIST {String.Join(",", channels)} {target}";
        }

        /// <summary>Names message</summary>
        public static String Names()
        {
            return "NAMES";
        }

        /// <summary>Names message.</summary>
        public static String Names(String channel)
        {
            return $"NAMES {channel}";
        }

        /// <summary>Names message.</summary>
        public static String Names(String[] channels)
        {
            return $"NAMES {String.Join(",", channels)}";
        }

        /// <summary>Names message.</summary>
        public static String Names(String channel, String target)
        {
            return $"NAMES {channel} {target}";
        }

        /// <summary>Names message.</summary>
        public static String Names(String[] channels, String target)
        {
            return $"NAMES {String.Join(",", channels)} {target}";
        }

        /// <summary>Topic message.</summary>
        public static String Topic(String channel)
        {
            return $"TOPIC {channel}";
        }

        /// <summary>Topic message.</summary>
        public static String Topic(String channel, String newtopic)
        {
            return $"TOPIC {channel} :{newtopic}";
        }

        /// <summary>Mode message.</summary>
        public static String Mode(String target)
        {
            return $"MODE {target}";
        }

        /// <summary>Mode message.</summary>
        public static String Mode(String target, String newmode)
        {
            return $"MODE {target} {newmode}" + target + " " + newmode;
        }

        /// <summary>Mode message.</summary>
        public static String Mode(String target, String[] newModes, String[] newModeParameters)
        {
            if (newModes == null)
            {
                throw new ArgumentNullException(nameof(newModes));
            }
            if (newModeParameters == null)
            {
                throw new ArgumentNullException(nameof(newModeParameters));
            }
            if (newModes.Length != newModeParameters.Length)
            {
                throw new ArgumentException("newModes and newModeParameters must have the same size.");
            }

            var newMode = new StringBuilder(newModes.Length);
            var newModeParameter = new StringBuilder();
            // as per RFC 3.2.3, maximum is 3 modes changes at once
            const Int32 maxModeChanges = 3;
            if (newModes.Length > maxModeChanges)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(target.Length),
                    newModes.Length,
                    $"Mode change list is too large (> {maxModeChanges})."
                );
            }
            
            for (var i = 0; i <= newModes.Length; i += maxModeChanges)
            {
                for (var j = 0; j < maxModeChanges; j++)
                {
                    if (i + j >= newModes.Length)
                    {
                        break;
                    }
                    newMode.Append(newModes[i + j]);
                }

                for (var j = 0; j < maxModeChanges; j++)
                {
                    if (i + j >= newModeParameters.Length)
                    {
                        break;
                    }
                    newModeParameter.Append(newModeParameters[i + j]);
                    newModeParameter.Append(" ");
                }
            }

            if (newModeParameter.Length <= 0) return Mode(target, newMode.ToString());

            // remove trailing space
            newModeParameter.Length--;
            newMode.Append(" ");
            newMode.Append(newModeParameter);

            return Mode(target, newMode.ToString());
        }

        /// <summary>Service message.</summary>
        public static String Service(String nickname, String distribution, String info)
        {
            return $"SERVICE {nickname} * {distribution} * * :{info}";
        }

        /// <summary>Invite message.</summary>
        public static String Invite(String nickname, String channel)
        {
            return $"INVITE {nickname} {channel}";
        }

        /// <summary>Who message.</summary>
        public static String Who()
        {
            return "WHO";
        }

        /// <summary>Who message.</summary>
        public static String Who(String mask)
        {
            return $"WHO {mask}";
        }

        /// <summary>Who message.</summary>
        public static String Who(String mask, Boolean ircop)
        {
            return ircop ? $"WHO {mask} o" : $"WHO {mask}";
        }

        /// <summary>Whois message.</summary>
        public static String Whois(String mask)
        {
            return $"WHOIS {mask}";
        }

        /// <summary>Whois message.</summary>
        public static String Whois(String[] masks)
        {
            return $"WHOIS {String.Join(",", masks)}";
        }

        /// <summary>Whois message.</summary>
        public static String Whois(String target, String mask)
        {
            return $"WHOIS {target} {mask}";
        }

        /// <summary>Whois message.</summary>
        public static String Whois(String target, String[] masks)
        {
            return $"WHOIS {target} {String.Join(",", masks)}";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String nickname)
        {
            return $"WHOWAS {nickname}";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String[] nicknames)
        {
            return $"WHOWAS {String.Join(",", nicknames)}";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String nickname, String count)
        {
            return $"WHOWAS {nickname} {count} ";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String[] nicknames, String count)
        {
            return $"WHOWAS {String.Join(",", nicknames)} {count} ";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String nickname, String count, String target)
        {
            return $"WHOWAS {nickname} {count} {target}";
        }

        /// <summary>Whowas message.</summary>
        public static String Whowas(String[] nicknames, String count, String target)
        {
            return $"WHOWAS {String.Join(",", nicknames)} {count} {target}";
        }

        /// <summary>Kill message.</summary>
        public static String Kill(String nickname, String comment)
        {
            return $"KILL {nickname} :{comment}";
        }

        /// <summary>Ping message.</summary>
        public static String Ping(String server)
        {
            return $"PING {server}";
        }

        /// <summary>Ping message.</summary>
        public static String Ping(String server, String server2)
        {
            return $"PING {server} {server2}";
        }

        /// <summary>Pong message.</summary>
        public static String Pong(String server)
        {
            return $"PONG {server}";
        }

        /// <summary>Pong message.</summary>
        public static String Pong(String server, String server2)
        {
            return $"PONG {server} {server2}";
        }

        /// <summary>Error message.</summary>
        public static String Error(String errormessage)
        {
            return $"ERROR :{errormessage}";
        }

        /// <summary>Away message.</summary>
        public static String Away()
        {
            return "AWAY";
        }

        /// <summary>Away message.</summary>
        public static String Away(String awaytext)
        {
            return $"AWAY :{awaytext}";
        }

        /// <summary>Rehash message</summary>
        public static String Rehash()
        {
            return "REHASH";
        }

        /// <summary>Die message.</summary>
        public static String Die()
        {
            return "DIE";
        }

        /// <summary>Restart message.</summary>
        public static String Restart()
        {
            return "RESTART";
        }

        /// <summary>Summon message.</summary>
        public static String Summon(String user)
        {
            return $"SUMMON {user}";
        }

        /// <summary>Summon message.</summary>
        public static String Summon(String user, String target)
        {
            return $"SUMMON {user} {target}" + user + " " + target;
        }

        /// <summary>Summon message.</summary>
        public static String Summon(String user, String target, String channel)
        {
            return $"SUMMON {user} {target} {channel}";
        }

        /// <summary>Users message.</summary>
        public static String Users()
        {
            return "USERS";
        }

        /// <summary>Users message.</summary>
        public static String Users(String target)
        {
            return $"USERS {target}";
        }

        /// <summary>Wallops message.</summary>
        public static String Wallops(String wallopstext)
        {
            return $"WALLOPS :{wallopstext}";
        }

        /// <summary>Userhost message.</summary>
        public static String Userhost(String nickname)
        {
            return $"USERHOST {nickname}";
        }

        /// <summary>Userhost message.</summary>
        public static String Userhost(String[] nicknames)
        {
            return $"USERHOST {String.Join(" ", nicknames)}";
        }

        /// <summary>Ison message.</summary>
        public static String Ison(String nickname)
        {
            return $"ISON {nickname}";
        }

        /// <summary>Ison message.</summary>
        public static String Ison(String[] nicknames)
        {
            return $"ISON {String.Join(" ", nicknames)}";
        }

        /// <summary>Quit message.</summary>
        public static String Quit()
        {
            return "QUIT";
        }

        /// <summary>Quit message.</summary>
        public static String Quit(String quitmessage)
        {
            return $"QUIT :{quitmessage}";
        }

        /// <summary>Squit message.</summary>
        public static String Squit(String server, String comment)
        {
            return $"SQUIT {server} :{comment}";
        }
    }
}