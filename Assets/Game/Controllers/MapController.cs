using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Pools;
using EcsRx.Unity.Components;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Game.Controllers
{
    public class MapController
    {

        private IPool _pool;

        public MapController(IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
        }

        public void start()
        {
            /*
                var tilemap2Go = GameObject.Find("Tilemap2");
                var tile = Resources.Load<TileBase>("TileAssets/Tile");
                var tilemap = tilemap2Go.GetComponent<Tilemap>();

                tilemap.SetTile(tilemap.WorldToCell(new Vector2(0, 0)), tile);
            */
        }
    }
}