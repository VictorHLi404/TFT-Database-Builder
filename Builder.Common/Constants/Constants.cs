using System.Runtime.ConstrainedExecution;

namespace Builder.Common.Constants;

public static class ConstantValues
{
    // current TFT set number. used for cleaning data recieved from riot API.
    public static readonly int TFT_SET_NUMBER = 14;
    // minimum # of games played for data to be qualified. Used to pull actually useful information from the database instead of noise.
    public static readonly int MINIMUM_GAMES_PLAYED = 10;
}