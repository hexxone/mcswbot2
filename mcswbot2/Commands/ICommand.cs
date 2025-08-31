namespace McswBot2.Commands
{
    internal abstract class Command
    {
        /// <summary>
        ///     Needs to be overwritten and return a low-cased string for representing when to call the command.
        /// </summary>
        /// <returns></returns>
        internal abstract string GetCommand();

        /// <summary>
        ///     Needs to be overwritten by the command-sepcific logic
        /// </summary>
        /// <param name="args">command arguments split by space</param>
        internal abstract void Call(CommandArgs args);
    }
}