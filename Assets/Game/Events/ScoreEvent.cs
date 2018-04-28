using EcsRx.Entities;

namespace Assets.Game.Events
{
	public class ScoreEvent
	{
		public IEntity source;
		public IEntity target;
	}
}