using ArchiVR.Application.Properties;

namespace ArchiVR.Application.Editable
{
    public class ArchiVRPOI
        : PropertiesBase

    {
        public readonly string[] DefaultNames =
        {
            "Attic",
            "Attic 1",
            "Attic 2",
            "Attic 3",
            "Attic 4",

            "Basement 1",
            "Basement 2",
            "Basement 3",
            "Bathroom",
            "Bathroom 1",
            "Bathroom 2",
            "Bedroom 1",
            "Bedroom 2",
            "Bedroom 3",
            "Bedroom 4",
            "Bedroom 5",

            "Dining Area",
            "Dining Area 1",
            "Dining Area 2",
            "Dining Area 3",

            "Entry",

            "Facilities Room",

            "Game Room",
            "Garage",

            "Hallway",
            "Hallway 1",
            "Hallway 2",
            "Hallway 3",

            "Kitchen",
            "Kitchen 1",
            "Kitchen 2",
            "Kitchen 3",
            "Kitchen 4",

            "Living Area",
            "Living Area 1",
            "Living Area 2",
            "Living Area 3",

            "Laundry Room",

            "Master Bedroom",

            "Office",
            "Office 1",
            "Office 2",
            "Office 3",
            "Outside Front Door",
            "Outside Front",
            "Outside Front-Left",
            "Outside Front-Right",
            "Outside Back",
            "Outside Back-Left",
            "Outside Back-Right",
            "Outside Left",
            "Outside Right",
            "Outside Terras",

            "Storage Room",

            "Toilet",
            "Toilet 1",
            "Toilet 2",
            "Toilet 3",
        };

        // Start is called before the first frame update
        void Start()
        {
            _properties = new IProperty[]
            {
                new ObjectNameProperty(gameObject, DefaultNames)
            };
        }
    }
}


