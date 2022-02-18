using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VorpInventory.Model
{
    [DataContract]
    public class DropOnRespawn
    {
        [DataMember(Name = "Money")]
        public bool Money;

        [DataMember(Name = "Weapons")]
        public bool Weapons;

        [DataMember(Name = "Items")]
        public bool Items;
    }

    [DataContract]
    public class MaxItemsInInventory
    {
        [DataMember(Name = "Weapons")]
        public int Weapons;

        [DataMember(Name = "Items")]
        public int Items;
    }

    [DataContract]
    public class WEAPONREVOLVERLEMAT
    {
        [DataMember(Name = "AMMO_REVOLVER")]
        public int AMMOREVOLVER;
    }

    [DataContract]
    public class StartItem
    {
        [DataMember(Name = "consumable_coffee")]
        public int ConsumableCoffee;

        [DataMember(Name = "consumable_peach")]
        public int ConsumablePeach;

        [DataMember(Name = "token")]
        public int Token;

        [DataMember(Name = "flag")]
        public int Flag;

        [DataMember(Name = "WEAPON_REVOLVER_LEMAT")]
        public List<WEAPONREVOLVERLEMAT> WEAPONREVOLVERLEMAT;
    }

    [DataContract]
    public class AmmoHash
    {
        [DataMember(Name = "AMMO_TOMAHAWK")]
        public int AMMOTOMAHAWK;

        [DataMember(Name = "AMMO_TOMAHAWK_IMPROVED")]
        public int AMMOTOMAHAWKIMPROVED;

        [DataMember(Name = "AMMO_TOMAHAWK_HOMING")]
        public int AMMOTOMAHAWKHOMING;

        [DataMember(Name = "AMMO_THROWING_KNIVES")]
        public int? AMMOTHROWINGKNIVES;

        [DataMember(Name = "AMMO_THROWING_KNIVES_IMPROVED")]
        public int? AMMOTHROWINGKNIVESIMPROVED;

        [DataMember(Name = "AMMO_THROWING_KNIVES_POISON")]
        public int? AMMOTHROWINGKNIVESPOISON;

        [DataMember(Name = "AMMO_ARROW")]
        public double? AMMOARROW;

        [DataMember(Name = "AMMO_ARROW_DYNAMITE")]
        public double? AMMOARROWDYNAMITE;

        [DataMember(Name = "AMMO_ARROW_FIRE")]
        public double? AMMOARROWFIRE;

        [DataMember(Name = "AMMO_ARROW_IMPROVED")]
        public double? AMMOARROWIMPROVED;

        [DataMember(Name = "AMMO_ARROW_POISON")]
        public double? AMMOARROWPOISON;

        [DataMember(Name = "AMMO_ARROW_SMALL_GAME")]
        public double? AMMOARROWSMALLGAME;

        [DataMember(Name = "AMMO_PISTOL")]
        public double? AMMOPISTOL;

        [DataMember(Name = "AMMO_PISTOL_EXPRESS")]
        public double? AMMOPISTOLEXPRESS;

        [DataMember(Name = "AMMO_PISTOL_EXPRESS_EXPLOSIVE")]
        public double? AMMOPISTOLEXPRESSEXPLOSIVE;

        [DataMember(Name = "AMMO_PISTOL_HIGH_VELOCITY")]
        public double? AMMOPISTOLHIGHVELOCITY;

        [DataMember(Name = "AMMO_PISTOL_SPLIT_POINT")]
        public double? AMMOPISTOLSPLITPOINT;

        [DataMember(Name = "AMMO_REVOLVER")]
        public double? AMMOREVOLVER;

        [DataMember(Name = "AMMO_REVOLVER_EXPRESS")]
        public double? AMMOREVOLVEREXPRESS;

        [DataMember(Name = "AMMO_REVOLVER_EXPRESS_EXPLOSIVE")]
        public double? AMMOREVOLVEREXPRESSEXPLOSIVE;

        [DataMember(Name = "AMMO_REVOLVER_HIGH_VELOCITY")]
        public double? AMMOREVOLVERHIGHVELOCITY;

        [DataMember(Name = "AMMO_REVOLVER_SPLIT_POINT")]
        public double? AMMOREVOLVERSPLITPOINT;

        [DataMember(Name = "AMMO_22")]
        public double? AMMO22;

        [DataMember(Name = "AMMO_REPEATER")]
        public double? AMMOREPEATER;

        [DataMember(Name = "AMMO_REPEATER_EXPRESS")]
        public double? AMMOREPEATEREXPRESS;

        [DataMember(Name = "AMMO_REPEATER_EXPRESS_EXPLOSIVE")]
        public double? AMMOREPEATEREXPRESSEXPLOSIVE;

        [DataMember(Name = "AMMO_REPEATER_HIGH_VELOCITY")]
        public double? AMMOREPEATERHIGHVELOCITY;

        [DataMember(Name = "AMMO_RIFLE")]
        public double? AMMORIFLE;

        [DataMember(Name = "AMMO_RIFLE_EXPRESS")]
        public double? AMMORIFLEEXPRESS;

        [DataMember(Name = "AMMO_RIFLE_EXPRESS_EXPLOSIVE")]
        public double? AMMORIFLEEXPRESSEXPLOSIVE;

        [DataMember(Name = "AMMO_RIFLE_HIGH_VELOCITY")]
        public double? AMMORIFLEHIGHVELOCITY;

        [DataMember(Name = "AMMO_RIFLE_SPLIT_POINT")]
        public double? AMMORIFLESPLITPOINT;

        [DataMember(Name = "AMMO_SHOTGUN")]
        public double? AMMOSHOTGUN;

        [DataMember(Name = "AMMO_SHOTGUN_BUCKSHOT_INCENDIARY")]
        public double? AMMOSHOTGUNBUCKSHOTINCENDIARY;

        [DataMember(Name = "AMMO_SHOTGUN_EXPRESS_EXPLOSIVE")]
        public double? AMMOSHOTGUNEXPRESSEXPLOSIVE;

        [DataMember(Name = "AMMO_SHOTGUN_SLUG")]
        public double? AMMOSHOTGUNSLUG;
    }

    [DataContract]
    public class CompsHash
    {
        [DataMember(Name = "w_melee_knife02_grip1")]
        public int WMeleeKnife02Grip1;

        [DataMember(Name = "w_pistol_semiauto01_sight1")]
        public int? WPistolSemiauto01Sight1;

        [DataMember(Name = "w_pistol_semiauto01_sight2")]
        public int? WPistolSemiauto01Sight2;

        [DataMember(Name = "w_pistol_semiauto01_grip1")]
        public int? WPistolSemiauto01Grip1;

        [DataMember(Name = "w_pistol_semiauto01_grip2")]
        public int? WPistolSemiauto01Grip2;

        [DataMember(Name = "w_pistol_semiauto01_grip3")]
        public int? WPistolSemiauto01Grip3;

        [DataMember(Name = "w_pistol_semiauto01_grip4")]
        public int? WPistolSemiauto01Grip4;

        [DataMember(Name = "w_pistol_semiauto01_clip")]
        public int? WPistolSemiauto01Clip;

        [DataMember(Name = "w_pistol_semiauto01_barrel1")]
        public int? WPistolSemiauto01Barrel1;

        [DataMember(Name = "w_pistol_semiauto01_barrel2")]
        public int? WPistolSemiauto01Barrel2;

        [DataMember(Name = "w_pistol_mauser01_sight1")]
        public int? WPistolMauser01Sight1;

        [DataMember(Name = "w_pistol_mauser01_sight2")]
        public int? WPistolMauser01Sight2;

        [DataMember(Name = "w_pistol_mauser01_grip1")]
        public int? WPistolMauser01Grip1;

        [DataMember(Name = "w_pistol_mauser01_grip2")]
        public int? WPistolMauser01Grip2;

        [DataMember(Name = "w_pistol_mauser01_grip3")]
        public int? WPistolMauser01Grip3;

        [DataMember(Name = "w_pistol_mauser01_grip4")]
        public int? WPistolMauser01Grip4;

        [DataMember(Name = "w_pistol_mauser01_clip")]
        public int? WPistolMauser01Clip;

        [DataMember(Name = "w_pistol_mauser01_barrel1")]
        public int? WPistolMauser01Barrel1;

        [DataMember(Name = "w_pistol_mauser01_barrel2")]
        public int? WPistolMauser01Barrel2;

        [DataMember(Name = "w_pistol_volcanic01_sight1")]
        public int? WPistolVolcanic01Sight1;

        [DataMember(Name = "w_pistol_volcanic01_sight2")]
        public int? WPistolVolcanic01Sight2;

        [DataMember(Name = "w_pistol_volcanic01_grip1")]
        public int? WPistolVolcanic01Grip1;

        [DataMember(Name = "w_pistol_volcanic01_grip2")]
        public int? WPistolVolcanic01Grip2;

        [DataMember(Name = "w_pistol_volcanic01_grip3")]
        public int? WPistolVolcanic01Grip3;

        [DataMember(Name = "w_pistol_volcanic01_grip4")]
        public int? WPistolVolcanic01Grip4;

        [DataMember(Name = "w_pistol_volcanic01_barrel01")]
        public int? WPistolVolcanic01Barrel01;

        [DataMember(Name = "w_pistol_volcanic01_barrel02")]
        public int? WPistolVolcanic01Barrel02;

        [DataMember(Name = "w_pistol_m189902_sight1")]
        public int? WPistolM189902Sight1;

        [DataMember(Name = "w_pistol_m189902_sight2")]
        public int? WPistolM189902Sight2;

        [DataMember(Name = "w_pistol_m189902_grip1")]
        public int? WPistolM189902Grip1;

        [DataMember(Name = "w_pistol_m189902_grip2")]
        public int? WPistolM189902Grip2;

        [DataMember(Name = "w_pistol_m189902_grip3")]
        public int? WPistolM189902Grip3;

        [DataMember(Name = "w_pistol_m189902_grip4")]
        public int? WPistolM189902Grip4;

        [DataMember(Name = "w_pistol_m189902_clip1")]
        public int? WPistolM189902Clip1;

        [DataMember(Name = "w_pistol_m189902_barrel01")]
        public int? WPistolM189902Barrel01;

        [DataMember(Name = "w_pistol_m189902_barrel02")]
        public int? WPistolM189902Barrel02;

        [DataMember(Name = "w_revolver_schofield01_sight1")]
        public int? WRevolverSchofield01Sight1;

        [DataMember(Name = "w_revolver_schofield01_sight2")]
        public int? WRevolverSchofield01Sight2;

        [DataMember(Name = "w_revolver_schofield01_grip1")]
        public int? WRevolverSchofield01Grip1;

        [DataMember(Name = "w_revolver_schofield01_grip2")]
        public int? WRevolverSchofield01Grip2;

        [DataMember(Name = "w_revolver_schofield01_grip3")]
        public int? WRevolverSchofield01Grip3;

        [DataMember(Name = "w_revolver_schofield01_grip4")]
        public int? WRevolverSchofield01Grip4;

        [DataMember(Name = "w_revolver_schofield01_barrel01")]
        public int? WRevolverSchofield01Barrel01;

        [DataMember(Name = "w_revolver_schofield01_barrel02")]
        public int? WRevolverSchofield01Barrel02;

        [DataMember(Name = "w_revolver_lemat01_sight1")]
        public int? WRevolverLemat01Sight1;

        [DataMember(Name = "w_revolver_lemat01_sight2")]
        public int? WRevolverLemat01Sight2;

        [DataMember(Name = "w_revolver_lemat01_grip1")]
        public int? WRevolverLemat01Grip1;

        [DataMember(Name = "w_revolver_lemat01_grip2")]
        public int? WRevolverLemat01Grip2;

        [DataMember(Name = "w_revolver_lemat01_grip3")]
        public int? WRevolverLemat01Grip3;

        [DataMember(Name = "w_revolver_lemat01_grip4")]
        public int? WRevolverLemat01Grip4;

        [DataMember(Name = "w_revolver_lemat01_barrel01")]
        public int? WRevolverLemat01Barrel01;

        [DataMember(Name = "w_revolver_lemat01_barrel02")]
        public int? WRevolverLemat01Barrel02;

        [DataMember(Name = "w_revolver_doubleaction01_sight1")]
        public int? WRevolverDoubleaction01Sight1;

        [DataMember(Name = "w_revolver_doubleaction01_sight2")]
        public int? WRevolverDoubleaction01Sight2;

        [DataMember(Name = "w_revolver_doubleaction01_grip1")]
        public int? WRevolverDoubleaction01Grip1;

        [DataMember(Name = "w_revolver_doubleaction01_grip2")]
        public int? WRevolverDoubleaction01Grip2;

        [DataMember(Name = "w_revolver_doubleaction01_grip3")]
        public int? WRevolverDoubleaction01Grip3;

        [DataMember(Name = "w_revolver_doubleaction01_grip4")]
        public int? WRevolverDoubleaction01Grip4;

        [DataMember(Name = "w_revolver_doubleaction01_grip5")]
        public int? WRevolverDoubleaction01Grip5;

        [DataMember(Name = "w_revolver_doubleaction01_barrel01")]
        public int? WRevolverDoubleaction01Barrel01;

        [DataMember(Name = "w_revolver_doubleaction01_barrel02")]
        public int? WRevolverDoubleaction01Barrel02;

        [DataMember(Name = "w_revolver_cattleman01_sight1")]
        public int? WRevolverCattleman01Sight1;

        [DataMember(Name = "w_revolver_cattleman01_sight2")]
        public int? WRevolverCattleman01Sight2;

        [DataMember(Name = "w_revolver_cattleman01_grip1")]
        public int? WRevolverCattleman01Grip1;

        [DataMember(Name = "w_revolver_cattleman01_grip2")]
        public int? WRevolverCattleman01Grip2;

        [DataMember(Name = "w_revolver_cattleman01_grip3")]
        public int? WRevolverCattleman01Grip3;

        [DataMember(Name = "w_revolver_cattleman01_grip4")]
        public int? WRevolverCattleman01Grip4;

        [DataMember(Name = "w_revolver_cattleman01_grip5")]
        public int? WRevolverCattleman01Grip5;

        [DataMember(Name = "w_revolver_cattleman01_barrel01")]
        public int? WRevolverCattleman01Barrel01;

        [DataMember(Name = "w_revolver_cattleman01_barrel02")]
        public int? WRevolverCattleman01Barrel02;

        [DataMember(Name = "w_repeater_pumpaction01_wrap1")]
        public int? WRepeaterPumpaction01Wrap1;

        [DataMember(Name = "w_repeater_pumpaction01_sight1")]
        public int? WRepeaterPumpaction01Sight1;

        [DataMember(Name = "w_repeater_pumpaction01_sight2")]
        public int? WRepeaterPumpaction01Sight2;

        [DataMember(Name = "w_repeater_pumpaction01_grip1")]
        public int? WRepeaterPumpaction01Grip1;

        [DataMember(Name = "w_repeater_pumpaction01_grip2")]
        public int? WRepeaterPumpaction01Grip2;

        [DataMember(Name = "w_repeater_pumpaction01_grip3")]
        public int? WRepeaterPumpaction01Grip3;

        [DataMember(Name = "w_repeater_pumpaction01_clip1")]
        public int? WRepeaterPumpaction01Clip1;

        [DataMember(Name = "w_repeater_pumpaction01_clip2")]
        public int? WRepeaterPumpaction01Clip2;

        [DataMember(Name = "w_repeater_pumpaction01_clip3")]
        public int? WRepeaterPumpaction01Clip3;

        [DataMember(Name = "w_repeater_winchester01_wrap1")]
        public int? WRepeaterWinchester01Wrap1;

        [DataMember(Name = "w_repeater_winchester01_sight1")]
        public int? WRepeaterWinchester01Sight1;

        [DataMember(Name = "w_repeater_winchester01_sight2")]
        public int? WRepeaterWinchester01Sight2;

        [DataMember(Name = "w_repeater_winchester01_grip1")]
        public int? WRepeaterWinchester01Grip1;

        [DataMember(Name = "w_repeater_winchester01_grip2")]
        public int? WRepeaterWinchester01Grip2;

        [DataMember(Name = "w_repeater_winchester01_grip3")]
        public int? WRepeaterWinchester01Grip3;

        [DataMember(Name = "w_repeater_henry01_wrap1")]
        public int? WRepeaterHenry01Wrap1;

        [DataMember(Name = "w_repeater_henry01_sight1")]
        public int? WRepeaterHenry01Sight1;

        [DataMember(Name = "w_repeater_henry01_sight2")]
        public int? WRepeaterHenry01Sight2;

        [DataMember(Name = "w_repeater_henry01_grip1")]
        public int? WRepeaterHenry01Grip1;

        [DataMember(Name = "w_repeater_henry01_grip2")]
        public int? WRepeaterHenry01Grip2;

        [DataMember(Name = "w_repeater_henry01_grip3")]
        public int? WRepeaterHenry01Grip3;

        [DataMember(Name = "w_repeater_evans01_wrap1")]
        public int? WRepeaterEvans01Wrap1;

        [DataMember(Name = "w_repeater_evans01_sight1")]
        public int? WRepeaterEvans01Sight1;

        [DataMember(Name = "w_repeater_evans01_sight2")]
        public int? WRepeaterEvans01Sight2;

        [DataMember(Name = "w_repeater_evans01_grip1")]
        public int? WRepeaterEvans01Grip1;

        [DataMember(Name = "w_repeater_evans01_grip2")]
        public int? WRepeaterEvans01Grip2;

        [DataMember(Name = "w_repeater_evans01_grip3")]
        public int? WRepeaterEvans01Grip3;

        [DataMember(Name = "w_repeater_carbine01_wrap1")]
        public int? WRepeaterCarbine01Wrap1;

        [DataMember(Name = "w_repeater_carbine01_sight1")]
        public int? WRepeaterCarbine01Sight1;

        [DataMember(Name = "w_repeater_carbine01_sight2")]
        public int? WRepeaterCarbine01Sight2;

        [DataMember(Name = "w_repeater_carbine01_grip1")]
        public int? WRepeaterCarbine01Grip1;

        [DataMember(Name = "w_repeater_carbine01_grip2")]
        public int? WRepeaterCarbine01Grip2;

        [DataMember(Name = "w_repeater_carbine01_grip3")]
        public int? WRepeaterCarbine01Grip3;

        [DataMember(Name = "w_repeater_carbine01_clip1")]
        public int? WRepeaterCarbine01Clip1;

        [DataMember(Name = "w_rifle_rollingblock01_wrap1")]
        public int? WRifleRollingblock01Wrap1;

        [DataMember(Name = "w_rifle_rollingblock01_sight2")]
        public int? WRifleRollingblock01Sight2;

        [DataMember(Name = "w_rifle_rollingblock01_sight1")]
        public int? WRifleRollingblock01Sight1;

        [DataMember(Name = "w_rifle_rollingblock01_grip1")]
        public int? WRifleRollingblock01Grip1;

        [DataMember(Name = "w_rifle_rollingblock01_grip2")]
        public int? WRifleRollingblock01Grip2;

        [DataMember(Name = "w_rifle_rollingblock01_grip3")]
        public int? WRifleRollingblock01Grip3;

        [DataMember(Name = "w_rifle_scopeinner01")]
        public int? WRifleScopeinner01;

        [DataMember(Name = "w_rifle_scope04")]
        public int? WRifleScope04;

        [DataMember(Name = "w_rifle_scope03")]
        public int? WRifleScope03;

        [DataMember(Name = "w_rifle_scope02")]
        public int? WRifleScope02;

        [DataMember(Name = "w_rifle_cs_strap01")]
        public int? WRifleCsStrap01;

        [DataMember(Name = "w_rifle_carcano01_wrap1")]
        public int? WRifleCarcano01Wrap1;

        [DataMember(Name = "w_rifle_carcano01_sight2")]
        public int? WRifleCarcano01Sight2;

        [DataMember(Name = "w_rifle_carcano01_sight1")]
        public int? WRifleCarcano01Sight1;

        [DataMember(Name = "w_rifle_carcano01_grip1")]
        public int? WRifleCarcano01Grip1;

        [DataMember(Name = "w_rifle_carcano01_grip2")]
        public int? WRifleCarcano01Grip2;

        [DataMember(Name = "w_rifle_carcano01_grip3")]
        public int? WRifleCarcano01Grip3;

        [DataMember(Name = "w_rifle_carcano01_clip")]
        public int? WRifleCarcano01Clip;

        [DataMember(Name = "w_rifle_carcano01_clip2")]
        public int? WRifleCarcano01Clip2;

        [DataMember(Name = "w_rifle_springfield01_wrap1")]
        public int? WRifleSpringfield01Wrap1;

        [DataMember(Name = "w_rifle_springfield01_sight2")]
        public int? WRifleSpringfield01Sight2;

        [DataMember(Name = "w_rifle_springfield01_sight1")]
        public int? WRifleSpringfield01Sight1;

        [DataMember(Name = "w_rifle_springfield01_grip1")]
        public int? WRifleSpringfield01Grip1;

        [DataMember(Name = "w_rifle_springfield01_grip2")]
        public int? WRifleSpringfield01Grip2;

        [DataMember(Name = "w_rifle_springfield01_grip3")]
        public int? WRifleSpringfield01Grip3;

        [DataMember(Name = "w_rifle_boltaction01_wrap1")]
        public int? WRifleBoltaction01Wrap1;

        [DataMember(Name = "w_rifle_boltaction01_sight1")]
        public int? WRifleBoltaction01Sight1;

        [DataMember(Name = "w_rifle_boltaction01_sight2")]
        public int? WRifleBoltaction01Sight2;

        [DataMember(Name = "w_rifle_boltaction01_grip1")]
        public int? WRifleBoltaction01Grip1;

        [DataMember(Name = "w_rifle_boltaction01_grip2")]
        public int? WRifleBoltaction01Grip2;

        [DataMember(Name = "w_rifle_boltaction01_grip3")]
        public int? WRifleBoltaction01Grip3;

        [DataMember(Name = "w_shotgun_semiauto01_wrap1")]
        public int? WShotgunSemiauto01Wrap1;

        [DataMember(Name = "w_shotgun_semiauto01_sight1")]
        public int? WShotgunSemiauto01Sight1;

        [DataMember(Name = "w_shotgun_semiauto01_sight2")]
        public int? WShotgunSemiauto01Sight2;

        [DataMember(Name = "w_shotgun_semiauto01_grip1")]
        public int? WShotgunSemiauto01Grip1;

        [DataMember(Name = "w_shotgun_semiauto01_grip2")]
        public int? WShotgunSemiauto01Grip2;

        [DataMember(Name = "w_shotgun_semiauto01_grip3")]
        public int? WShotgunSemiauto01Grip3;

        [DataMember(Name = "w_shotgun_semiauto01_barrel1")]
        public int? WShotgunSemiauto01Barrel1;

        [DataMember(Name = "w_shotgun_semiauto01_barrel2")]
        public int? WShotgunSemiauto01Barrel2;

        [DataMember(Name = "w_shotgun_sawed01_wrap1")]
        public int? WShotgunSawed01Wrap1;

        [DataMember(Name = "w_shotgun_sawed01_sight1")]
        public int? WShotgunSawed01Sight1;

        [DataMember(Name = "w_shotgun_sawed01_sight2")]
        public int? WShotgunSawed01Sight2;

        [DataMember(Name = "w_shotgun_sawed01_grip1")]
        public int? WShotgunSawed01Grip1;

        [DataMember(Name = "w_shotgun_sawed01_grip2")]
        public int? WShotgunSawed01Grip2;

        [DataMember(Name = "w_shotgun_sawed01_grip3")]
        public int? WShotgunSawed01Grip3;

        [DataMember(Name = "w_shotgun_sawed01_stock1")]
        public int? WShotgunSawed01Stock1;

        [DataMember(Name = "w_shotgun_sawed01_stock2")]
        public int? WShotgunSawed01Stock2;

        [DataMember(Name = "w_shotgun_sawed01_stock3")]
        public int? WShotgunSawed01Stock3;

        [DataMember(Name = "w_shotgun_repeating01_wrap1")]
        public int? WShotgunRepeating01Wrap1;

        [DataMember(Name = "w_shotgun_repeating01_sight1")]
        public int? WShotgunRepeating01Sight1;

        [DataMember(Name = "w_shotgun_repeating01_sight2")]
        public int? WShotgunRepeating01Sight2;

        [DataMember(Name = "w_shotgun_repeating01_grip1")]
        public int? WShotgunRepeating01Grip1;

        [DataMember(Name = "w_shotgun_repeating01_grip2")]
        public int? WShotgunRepeating01Grip2;

        [DataMember(Name = "w_shotgun_repeating01_grip3")]
        public int? WShotgunRepeating01Grip3;

        [DataMember(Name = "w_shotgun_repeating01_barrel1")]
        public int? WShotgunRepeating01Barrel1;

        [DataMember(Name = "w_shotgun_repeating01_barrel2")]
        public int? WShotgunRepeating01Barrel2;

        [DataMember(Name = "w_shotgun_pumpaction01_wrap1")]
        public int? WShotgunPumpaction01Wrap1;

        [DataMember(Name = "w_shotgun_pumpaction01_sight1")]
        public int? WShotgunPumpaction01Sight1;

        [DataMember(Name = "w_shotgun_pumpaction01_sight2")]
        public int? WShotgunPumpaction01Sight2;

        [DataMember(Name = "w_shotgun_pumpaction01_grip1")]
        public int? WShotgunPumpaction01Grip1;

        [DataMember(Name = "w_shotgun_pumpaction01_grip2")]
        public int? WShotgunPumpaction01Grip2;

        [DataMember(Name = "w_shotgun_pumpaction01_grip3")]
        public int? WShotgunPumpaction01Grip3;

        [DataMember(Name = "w_shotgun_pumpaction01_barrel1")]
        public int? WShotgunPumpaction01Barrel1;

        [DataMember(Name = "w_shotgun_pumpaction01_barrel2")]
        public int? WShotgunPumpaction01Barrel2;

        [DataMember(Name = "w_shotgun_pumpaction01_clip1")]
        public int? WShotgunPumpaction01Clip1;

        [DataMember(Name = "w_shotgun_pumpaction01_clip2")]
        public int? WShotgunPumpaction01Clip2;

        [DataMember(Name = "w_shotgun_pumpaction01_clip3")]
        public int? WShotgunPumpaction01Clip3;

        [DataMember(Name = "w_shotgun_doublebarrel01_wrap1")]
        public int? WShotgunDoublebarrel01Wrap1;

        [DataMember(Name = "w_shotgun_doublebarrel01_sight1")]
        public int? WShotgunDoublebarrel01Sight1;

        [DataMember(Name = "w_shotgun_doublebarrel01_sight2")]
        public int? WShotgunDoublebarrel01Sight2;

        [DataMember(Name = "w_shotgun_doublebarrel01_grip1")]
        public int? WShotgunDoublebarrel01Grip1;

        [DataMember(Name = "w_shotgun_doublebarrel01_grip2")]
        public int? WShotgunDoublebarrel01Grip2;

        [DataMember(Name = "w_shotgun_doublebarrel01_grip3")]
        public int? WShotgunDoublebarrel01Grip3;

        [DataMember(Name = "w_shotgun_doublebarrel01_barrel1")]
        public int? WShotgunDoublebarrel01Barrel1;

        [DataMember(Name = "w_shotgun_doublebarrel01_barrel2")]
        public int? WShotgunDoublebarrel01Barrel2;

        [DataMember(Name = "w_shotgun_doublebarrel01_mag1")]
        public int? WShotgunDoublebarrel01Mag1;

        [DataMember(Name = "w_shotgun_doublebarrel01_mag2")]
        public int? WShotgunDoublebarrel01Mag2;

        [DataMember(Name = "w_shotgun_doublebarrel01_mag3")]
        public int? WShotgunDoublebarrel01Mag3;

        [DataMember(Name = "w_camera_inner01")]
        public int? WCameraInner01;
    }

    [DataContract]
    public class Weapon
    {
        [DataMember(Name = "Name")]
        public string Name;

        [DataMember(Name = "HashName")]
        public string HashName;

        [DataMember(Name = "WeaponModel")]
        public string WeaponModel;

        [DataMember(Name = "Price")]
        public double Price;

        [DataMember(Name = "AmmoHash")]
        public List<AmmoHash> AmmoHash;

        [DataMember(Name = "CompsHash")]
        public List<CompsHash> CompsHash;
    }

    [DataContract]
    public class Config
    {
        [DataMember(Name = "defaultlang")]
        public string Defaultlang;

        [DataMember(Name = "OpenKey")]
        public string OpenKey;

        [DataMember(Name = "PickupKey")]
        public string PickupKey;

        [DataMember(Name = "DropOnRespawn")]
        public DropOnRespawn DropOnRespawn;

        [DataMember(Name = "MaxItemsInInventory")]
        public MaxItemsInInventory MaxItemsInInventory;

        [DataMember(Name = "startItems")]
        public List<StartItem> StartItems;

        [DataMember(Name = "Weapons")]
        public List<Weapon> Weapons;
    }


}