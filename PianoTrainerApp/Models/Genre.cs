using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public enum Genre
    {
        Classical,
        Pop,
        Metal,
        Rock,
        Alternative,
        Folk,
        Country,
        Jazz,
        Reggae,
        Latin,
        World,
        [Description("R&B")]
        RB,
        Electronic,
        Blues,
        Funk,
        [Description("Hip hop")]
        HipHop,
        Soundtrack
    }
}
