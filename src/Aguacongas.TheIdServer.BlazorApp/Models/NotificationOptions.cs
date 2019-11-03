﻿using System;

namespace Aguacongas.TheIdServer.BlazorApp.Models
{
    public class NotificationOptions
    {
        public Guid Id { get; set; }

        public bool Animation { get; set; } = true;

        public bool Autohide { get; set; } = true;

        public int Delay { get; set; }
    }
}
