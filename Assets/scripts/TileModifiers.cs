using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public interface ITileCombatModifiers
{
    float RangeMultiplier { get; }
    float DefenceMultiplier { get; }
}

public interface ITileEconomyModifiers
{
    int CashValue { get; }
    int ScoreValues { get; }
}

public interface ITileObserver
{
    void TileModsChanged(ITileCombatModifiers mods);
}

public class TiletModifiers : ITileCombatModifiers, ITileEconomyModifiers
{
    private float rangeMultiplier;
    private float defenceMultiplier;
    private int cashValue;
    private int scoreValue;

    public TiletModifiers(float range, float defense, int cash, int score)
    {
        rangeMultiplier = range;
        defenceMultiplier = defense;
        cashValue = cash;
        scoreValue = score;
    }
    public float RangeMultiplier
    {
        get { return rangeMultiplier; }
    }
    public float DefenceMultiplier
    {
        get { return defenceMultiplier; }
    }
    public int CashValue
    {
        get { return cashValue; }
    }
    public int ScoreValues
    {
        get { return scoreValue; }
    }
}

public static class TileModifiersFactory
{
    private static List<float> sPlainRangeModifiers = new List<float> { 1, 1, 1, 1 };
    private static List<float> sPlainsDefenseModifiers = new List<float> { 1, 1.2f, 1.4f, 1.6f };
    private static List<int> sPlainsCashValues = new List<int> { 0, 0, 0, 0 };
    private static List<int> sPlainsScoreValues = new List<int> { 1, 1, 1, 1 };

    private static List<float> sHillRangeModifiers = new List<float> { 1.25f, 1.5f, 1.75f, 2f };
    private static List<float> sHillDefenseModifiers = new List<float> { 1.25f, 1.5f, 1.75f, 2 };
    private static List<int> sHillCashValues = new List<int> { 0, 0, 0, 0 };
    private static List<int> sHillScoreValues = new List<int> { 2, 3, 4, 5 };

    private static List<float> sMineRangeModifiers = new List<float> { 0.6f, 0.8f, 1, 1.2f };
    private static List<float> sMineDefenseModifiers = new List<float> { 0.6f, 0.8f, 1, 1.2f };
    private static List<int> sMineCashValues = new List<int> { 10, 20, 30, 40 };
    private static List<int> sMineScoreValues = new List<int> { 1, 2, 4, 6 };

    public static TiletModifiers Create(TileType type, int rank)
    {
        if (rank < 0 || rank > Tile.MAX_TILE_RANK)
            throw new ArgumentException();

        switch (type)
        {
            case TileType.Plain:
                return new TiletModifiers(sPlainRangeModifiers[rank], sPlainsDefenseModifiers[rank],
                    sPlainsCashValues[rank], sPlainsScoreValues[rank]);
            case TileType.Hill:
                return new TiletModifiers(sHillRangeModifiers[rank], sHillDefenseModifiers[rank],
                    sHillCashValues[rank], sHillScoreValues[rank]);
            case TileType.Mine:
                return new TiletModifiers(sMineRangeModifiers[rank], sMineDefenseModifiers[rank],
                    sMineCashValues[rank], sMineScoreValues[rank]);
        }
        throw new ArgumentException();
    }
}
