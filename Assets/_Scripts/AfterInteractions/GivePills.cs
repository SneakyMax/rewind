﻿namespace Assets._Scripts.AfterInteractions
{
    public class GivePills : AfterInteraction
    {
        public override void Trigger()
        {
            Player.Instance.AddPill();
        }
    }
}