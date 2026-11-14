using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths
{
    public static class RevealChanges
    {
        static readonly int range = 2;

        public static void RevealRocks(int i, LevelComponent level)
        {
            int dimensionsScaled = level.dimensionsScaled;

            int checkRange = (range * 2) + 1;

            int posY = i / dimensionsScaled;
            int posX = i % dimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / dimensionsScaled;
            int checkX = i % dimensionsScaled;
            checkY = math.clamp((dimensionsScaled + 1) - checkY, 0, checkRange);
            checkX = math.clamp((dimensionsScaled + 1) - checkX, 0, checkRange);



            //int checkIndex = i - range - range * levelDimensionsScaled;

            int checkIndex = i - posX - posY * dimensionsScaled;
            for (int y = 0; y < checkY; y++)
            {
                for (int x = 0; x < checkX; x++)
                {
                    LevelGridTile tile = level.renderGrid[checkIndex + x];
                    tile.hiddenTypeStyleAmount = ~1 & tile.hiddenTypeStyleAmount;
                    level.renderGrid[checkIndex + x] = tile;
                }
                checkIndex += dimensionsScaled;
            }
        }

        public static bool IsRevealed(int i, LevelComponent level)
        {
            int dimensionsScaled = level.dimensionsScaled;

            int checkRange = (range * 2) + 1;

            int posY = i / dimensionsScaled;
            int posX = i % dimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / dimensionsScaled;
            int checkX = i % dimensionsScaled;
            checkY = math.clamp((dimensionsScaled + 1) - checkY, 0, checkRange);
            checkX = math.clamp((dimensionsScaled + 1) - checkX, 0, checkRange);



            int checkIndex = i - posX - posY * dimensionsScaled;
            for (int y = 0; y < checkY; y++)
            {
                for (int x = 0; x < checkX; x++)
                {
                    if (checkIndex + x == i)
                        continue;

                    LevelGridTile tile = level.renderGrid[checkIndex + x];
                    if (tile.TType == TileType.Road)
                        return true;
                }
                checkIndex += dimensionsScaled;
            }
            return false;
        }
    }
}
