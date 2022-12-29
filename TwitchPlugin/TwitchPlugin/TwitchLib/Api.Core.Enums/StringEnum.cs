namespace TwitchLib.Api.Core.Enums
{
    using System;

    ///<summary>Really cool way of doing string enums, credits to RogueException (https://github.com/RogueException)</summary>
    public abstract class StringEnum
    {
        /// <summary>Value of enum</summary>
        public String Value { get; }

        /// <summary>StringEnum constructor.</summary>
        protected StringEnum(String value)
        {
            this.Value = value;
        }

        /// <summary>Returns string value for overriden ToString()</summary>
        /// <returns>Enum value</returns>
        public override String ToString() => this.Value;
    }
}
