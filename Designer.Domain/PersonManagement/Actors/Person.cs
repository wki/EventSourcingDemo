﻿using System;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class Person : DurableActor<int>
    {
        public Person()
        {
        }
    }
}
