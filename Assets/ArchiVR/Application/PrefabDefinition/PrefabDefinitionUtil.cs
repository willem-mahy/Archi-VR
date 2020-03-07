using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiVR.Application.PrefabDefinition
{
    class PrefabDefinitionUtil
    {
        public static List<ObjectPrefabDefinition> GetPropPrefabDefinitions()
        {
            return new List<ObjectPrefabDefinition>()
            {
                new ObjectPrefabDefinition("Lavabo Dubbel 140x50x50",                           "ArchiVR/Prefab/Architectural/Furniture/Bathroom/Lavabo Dubbel 140x50x50"),
                new ObjectPrefabDefinition("Mirror 142x70",                                     "ArchiVR/Prefab/Architectural/Furniture/Bathroom/Mirror 142x70"),
                new ObjectPrefabDefinition("Radiator_Electric_Acova_Regate_El_Air_(50x135cm)",  "ArchiVR/Prefab/Architectural/Furniture/Bathroom/Radiator_Electric_Acova_Regate_El_Air_(50x135cm)"),
                new ObjectPrefabDefinition("Douche 1",                                          "ArchiVR/Prefab/Architectural/Furniture/Bathroom/Douche/Douche 1"),

                new ObjectPrefabDefinition("Cooking Plates",            "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Cooking Plates"),
                new ObjectPrefabDefinition("Dampkap",                   "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Dampkap"),
                new ObjectPrefabDefinition("Kitchen Foucet",            "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Kitchen Foucet"),
                new ObjectPrefabDefinition("Kitchen Sink",              "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Kitchen Sink"),
                new ObjectPrefabDefinition("Stool Modern White 01",     "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Stool Modern White 01"),
                new ObjectPrefabDefinition("Toaster",                   "ArchiVR/Prefab/Architectural/Furniture/Kitchen/Toaster"),

                new ObjectPrefabDefinition("Bed 1P", "ArchiVR/Prefab/Architectural/Furniture/Bedroom/Bed_1P"),
                new ObjectPrefabDefinition("Bed 2P", "ArchiVR/Prefab/Architectural/Furniture/Bedroom/Bed_2P_01"),

                new ObjectPrefabDefinition("Chair 01",  "ArchiVR/Prefab/Architectural/Furniture/Dining Area/Chair 01"),
                new ObjectPrefabDefinition("Table 01",  "ArchiVR/Prefab/Architectural/Furniture/Dining Area/Table 01"),
                new ObjectPrefabDefinition("Vases",     "ArchiVR/Prefab/Architectural/Furniture/Dining Area/Vases"),

                new ObjectPrefabDefinition("Dryer",     "ArchiVR/Prefab/Architectural/Furniture/Electro/Dryer"),

                new ObjectPrefabDefinition("Sofa 1P",    "ArchiVR/Prefab/Architectural/Furniture/Living Area/Sofa 1P"),
                new ObjectPrefabDefinition("Sofa 2P",    "ArchiVR/Prefab/Architectural/Furniture/Living Area/Sofa 2P"),
                new ObjectPrefabDefinition("TV Philips", "ArchiVR/Prefab/Architectural/Furniture/Living Area/TV Philips"),
                new ObjectPrefabDefinition("TV Meubel",  "ArchiVR/Prefab/Architectural/Furniture/Living Area/TV Meubel 600x2800x500"),

                new ObjectPrefabDefinition("Herbs1", "ArchiVR/Prefab/Architectural/Furniture/Plants/Herbs1"),
                new ObjectPrefabDefinition("Herbs2", "ArchiVR/Prefab/Architectural/Furniture/Plants/Herbs2"),

                new ObjectPrefabDefinition("Drying Rack",    "ArchiVR/Prefab/Architectural/Furniture/Sanitair/Drying Rack 01 White 207x57x115"),

                new ObjectPrefabDefinition("Laundry Baskets",    "ArchiVR/Prefab/Architectural/Furniture/Storage/Laundry_Baskets"),
                new ObjectPrefabDefinition("Storage Rack White", "ArchiVR/Prefab/Architectural/Furniture/Storage/Storage_Rack_White_01"),
                new ObjectPrefabDefinition("Storage Rack Wood",  "ArchiVR/Prefab/Architectural/Furniture/Storage/Storage_Rack_Wood_01"),

                new ObjectPrefabDefinition("Lavabo VINOVA 10002 47x13x26",   "ArchiVR/Prefab/Architectural/Furniture/Toilet/Lavabo VINOVA 10002 47x13x26"),
                new ObjectPrefabDefinition("Toilet Pot 01",                  "ArchiVR/Prefab/Architectural/Furniture/Toilet/Toilet Pot 01"),
                new ObjectPrefabDefinition("Toilet Roll Holder 01",          "ArchiVR/Prefab/Architectural/Furniture/Toilet/Toilet Roll Holder 01"),

                new ObjectPrefabDefinition("Lavender", "ArchiVR/Prefab/Architectural/Vegetation/Bushes/Lavender"),

                new ObjectPrefabDefinition("Audi A6 01 Blue", "ArchiVR/Prefab/Vehicle/Civilian/Audi/Car Audi A6 01 Blue"),
            };
        }
    }
}
