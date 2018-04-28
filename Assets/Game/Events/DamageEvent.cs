using EcsRx.Entities;

namespace Assets.Game.Events
{
	public class DamageEvent
	{
		public IEntity source;
		public IEntity target;
	}
}