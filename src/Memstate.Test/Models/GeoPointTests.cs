using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Memstate.Models.Geo;
using System.IO;

namespace Memstate.Test.Models
{
    [TestFixture]
    public class GeoPointTests
    {
        private static GeoSpatialIndex<String> _places;

        public class GeoLocation
        {
            public readonly string Name;
            public readonly GeoPoint Point;

            public GeoLocation(string name, GeoPoint point)
            {
                Point = point;
                Name = name;
            }

            public override string ToString()
            {
                return Name + ":" + Point.Latitude + ":" + Point.Longitude;
            }
        }

        private static IEnumerable<object[]> TestCases()
        {
            yield return new object[]
                             {
                                 //Palermo
                                 new GeoPoint(38.115556, 13.361389), 
                                 //Catania
                                 new GeoPoint(37.502669, 15.087269),

                                 //expected distance between Palermo and Catania
                                 166.27415156960038
                             };
        }

        private static IEnumerable<GeoLocation> TestData()
        {
            using (var reader = new StringReader(raw))
            {
                while(true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    var arr = line.Split('\t');
                    var name = arr[0];

                    var lat = Double.Parse(arr[1], CultureInfo.InvariantCulture);
                    var lon = Double.Parse(arr[2], CultureInfo.InvariantCulture);
                    var point = new GeoPoint(lat, lon);
                    yield return new GeoLocation(name, point);
                }
            }
        }

        [OneTimeSetUp]
        public static void Setup()
        {
            _places = new GeoSpatialIndex<string>();
            foreach (var location in TestData())
            {
                _places.Add(location.Name, location.Point);
            }
        }

        [Test]
        public void GeoPointToString()
        {
            var gp = new GeoPoint(10,20);
            Assert.AreEqual("[10:20]", gp.ToString());
        }

        [Test]
        public void DistanceToSelfShouldBeZero()
        {
            var gp = new GeoPoint(10, 10);
            Assert.AreEqual(0D, gp.DistanceTo(gp).Radians, 0.0001);
        }

        [Test, TestCaseSource("TestCases")]
        public void Distance(GeoPoint a, GeoPoint b, double expectedDistance)
        {
            var actual = GeoPoint.Distance(a, b);
            var actualInverse = GeoPoint.Distance(b, a);
            Assert.AreEqual(actual, actualInverse, "dist(b,a) should equal dist(a,b)");
            double faultTolerance = expectedDistance * 0.005;
            Assert.AreEqual(expectedDistance, actual.ToKilometers(), faultTolerance);
        }

        [Test, TestCaseSource("TestData")]
        public void WithinRadiusTest(GeoLocation sample)
        {
            const double radius = 100;

            Console.WriteLine($"Searching for places within {radius} km of {sample.Name}");
            var within = _places.WithinRadius(sample.Point, radius).OrderBy(p => p.Value).ToArray();

            Console.WriteLine("Found " + within.Length);
            foreach (var keyValuePair in within)
            {
                var kms = keyValuePair.Value.ToKilometers();
                Console.WriteLine($"{keyValuePair.Key} at distance {kms} km" );
                Assert.IsTrue(keyValuePair.Value.ToKilometers() <= radius);
            }

            //Double check and print any errors
            var withinNames = new HashSet<string>(within.Select(kvp => kvp.Key));
            int failures = 0;
            foreach (var keyValuePair in _places)
            {
                var name = keyValuePair.Key;
                var distance = GeoPoint.Distance(sample.Point, keyValuePair.Value);
                if (distance.ToKilometers() > radius && withinNames.Contains(name))
                {
                    failures++;
                    Console.WriteLine("false positive: " + name + ", d=" + distance );
                }
                if (distance.ToKilometers() <= radius && !withinNames.Contains(name))
                {
                    failures++;
                    Console.WriteLine("false negative: " + name + ", d=" + distance);
                }
                
            }
            Assert.IsTrue(failures == 0, "failures: " + failures);
        }


        //swedish cities/locations taken from http://www.geonames.org/
        private static string raw = @"Gåshällan	63.65	20.25
Viggen	63.63333	20.23333
Kallen	63.64167	20.21667
Sörbölekobbarna	63.65	20.18333
Tveklova	63.63333	20.2
Vitskäret	63.65	20.3
Tarv	63.65	20.26667
Sundet	63.66667	20.26667
Flakaskär	63.65833	20.21667
Skatan	63.65124	20.11561
Utanånäset	63.65277	20.10354
Ovalen	63.65	20.08333
Nygrundet	63.66667	20.05
Lillskärsudden	63.66667	20.03333
Sörmjöle Havsbad	63.66667	20
Revet	63.65334	20.12273
Yttre-Åhällan	63.63333	20.11667
Finngrundet	63.65	20.03333
Vitögern	63.76667	20.58333
Harrgrundet	63.775	20.65
Karet	63.75	20.63333
Nör-Tärnögern	63.775	20.61667
Sör-Tärnögern	63.76667	20.625
Sävar-Tärnögern	63.76667	20.6
Sävarfjärden	63.76667	20.58333
Långrevskäret	63.76667	20.5
Lappgrundet	63.75	20.51667
Lyckan	63.76667	20.48333
Kankholmen	63.76667	20.46667
Nörd-Petersgrundet	63.75	20.5
Stor-Kas	63.75	20.43333
Utbörtingen	63.73333	20.43333
Kas	63.75	20.43333
Sillviken	63.76667	20.45
Tavlefjärden	63.75	20.41667
Sofiehem	63.80833	20.3
Stor-Sandskär	63.76667	20.31667
Lill-Sandskär	63.75	20.33333
Villanäs	63.75	20.35
Flisbergsgrundet	63.75	20.33333
Tuvan	63.75	20.31667
Långgärdet	63.73333	20.48333
Rovögern	63.73333	20.53333
Klintgrundet	63.73333	20.61667
Gripgrundet	63.73333	20.61667
Masken	63.70833	20.6
Nyhällsgrundet	63.7	20.55
Stoppregeln	63.725	20.48333
Brinken	63.73333	20.46667
Bakom	63.71667	20.46667
Buten	63.71667	20.44167
Foten	63.71667	20.46667
Ensam	63.7	20.48333
Knapen	63.71667	20.46667
Storklyvan	63.7	20.43333
Lillklyvan	63.7	20.44167
Lövölandet	63.71667	20.41667
Bortre Nyviken	63.71667	20.31667
Stommen	63.68333	21
Splittran	63.68333	20.94167
Skvalen	63.66667	20.9
Lill-Halörsgrundet	63.68333	20.89167
Stor-Halörsgrundet	63.68333	20.9
Halörsskatan	63.68333	20.88333
Rössgrundet	63.68333	20.86667
Vilan	63.675	20.81667
Södra Knivingskallen	63.68333	20.8
Vidbrännan	63.68333	20.81667
Utskottet	63.68333	20.78333
Malgrundkallen	63.66667	20.8
Väntansgrundet	63.66667	20.6
Eriksgrundet	63.68333	20.61667
Stråket	63.7	20.6
Strängen	63.7	20.58333
Krongrundet	63.68333	20.6
Algrundet	63.69167	20.43333
Trutskär	63.68333	20.45
Lövö-Storbådan	63.68333	20.46667
Lillbådan	63.7	20.46667
Brännölandet	63.7	20.41667
Tärnögern	63.7	20.4
Prejaren	63.68333	20.475
Stor-Haddingen	63.66667	20.41667
Kroklandet	63.68333	20.4
Byllan	63.68333	20.35
Vitskär	63.66667	20.38333
Patholmsviken	63.69167	20.36667
Långhalsudden	63.7	20.31667
Klubbgrundet	63.68333	20.31667
Klubben	63.68333	20.31667
Malgrundet	63.66667	20.81667
Långögern	63.65	20.81667
Trehövda	63.66667	20.55
Desterrogrundet	63.66667	20.53333
Hättan	63.66667	20.45
Skeppsgrundet	63.66667	20.41667
Isaksgrundet	63.65833	20.43333
Röran	63.65	20.40833
Evagrundet	63.65833	20.38333
Åhällarna	63.66667	20.38333
Domaldgrundet	63.66667	20.35
Revet	63.65833	20.35
Bredskär	63.66667	20.31667
Bredskärssten	63.65	20.31667
Dynan	63.65	20.33333
Vingelgrundet	63.66667	20.9
Fyrkanten	63.65833	20.91667
Vakaren	63.65	20.95
Bocken	63.63333	20.91667
Storbrännan	63.65	20.9
Bullerbotten	63.65	20.86667
Stenbredan	63.63333	20.78333
Trollholmen	63.63333	20.81667
Säcken	63.65	20.76667
Liggaren	63.63333	20.76667
Nygrundsholmen	63.63333	20.43333
Utposten	63.63333	20.45833
Vattingen	63.65	20.44167
Kråksten	63.65	20.43333
Medholmen	63.65	20.41667
Ytholmen	63.65	20.4
Klinten	63.65	20.35
Storbränningen	63.63333	20.36667
Lillbådan	63.65	20.34167
Långan	63.65	20.35
Väjan	63.65	20.31667
Obbolstenarna	63.65	20.3
Skrapan	63.63333	20.3
Kedjegrunden	63.63333	20.86667
Malskärsögern	63.63333	20.83333
Fulingen	63.625	20.81667
Gråsjälbådan	63.63333	20.81667
Mellangrundet	63.63333	20.8
Malhösarna	63.61667	20.78333
Gaddhällar	63.61667	20.76667
Springaren	63.61667	20.46667
Port Arturs Grund	63.63333	20.41667
Ullmansholmarna	63.63333	20.41667
Obbola-Storbådan	63.63333	20.35833
Knappraden	63.63333	20.36667
Majesticsgrundet	63.61667	20.38333
Per-Jonsgrundet	63.61667	20.35
Karet	63.63333	20.31667
Rönnblomsgrundet	63.63333	20.31667
Knallen	63.63333	20.31667
Kaxen	63.61667	20.86667
Sjösänkan	63.61667	20.9
Förposten	63.61667	20.88333
Tvåknösen	63.59167	20.85
Maltaggarna	63.6	20.80833
Trestena	63.6	20.83333
Malgrundsklacken	63.6	20.81667
Knölgrunden	63.61667	20.825
Svartbådaholmen	63.58333	20.78333
Malgrundet	63.6	20.8
Kvisslan	63.61667	20.36667
Bocken	63.61667	20.33333
Stygnet	63.61667	20.31667
Känningen	63.6	20.33333
Hampusgrundet	63.58333	20.35
Rafflan	63.58333	20.83333
Stollen	63.58333	20.83333
Tinnarna	63.56667	20.83333
Höggrensgrundet	63.58333	20.80833
Erik-Augustgrundet	63.58333	20.75
Sjöbrottet	63.56667	20.75833
Hugingrundet	63.56667	20.76667
Sönnerstgrundkallen	63.575	20.73333
Emanuelsgrundet	63.56667	20.38333
Kryssgrundet	63.58333	20.31667
Nordvalensgrundet	63.53333	20.8
Gerdasgrundet	63.51667	20.76667
Knösen	63.53333	20.4
Alfhildsgrundet	63.54167	20.3
Gurlisgrundet	63.48333	20.78333
Fiskargrundet	63.46667	20.775
Gunvorsgrund	63.48333	20.45
Sänket	63.48333	20.41667
Utlöparen	63.475	20.35
Hällkallan	63.43333	20.96667
Medelkallan	63.40833	20.85
Teutoniagrundet	63.45	20.75
Östra Snipansgrundet	63.425	20.78333
Västra Snipansgrundet	63.43333	20.73333
Odelsgrund	63.43333	20.55833
Klubben	64.13333	20.975
Kungsöhällan	64.13333	20.98333
Lill-Getskär	64.15	20.98333
Ölandet	64.15	20.96667
Klubbhällan	64.13333	20.98333
Skravelhällan	64.15	21
Slingran	64.13333	21.00833
Stor-Getskär	64.15	20.98333
Olsviken	64.15833	21
Lillgrundet	64.15	21
Sikeåfjärden	64.15	20.975
Svinskataudden	64.1	20.96667
Lägdviken	64.15	20.96667
Sandviken	64.11667	20.95
Svartstenen	64.125	20.98333
Storholmen	64.11667	20.98333
Ricklestenen	64.11667	21
Fisteln	64.11667	21.01667
Svalpan	64.1	20.99167
Säcken	64.1	21
Bådan	64.08333	20.98333
Fagerviken	64.1	20.95
Fagervikholmen	64.09167	20.96667
Avaviken	64.08333	20.95
Klubben	64.08333	20.95833
Hösen	64.075	20.98333
Trekanten	64.075	20.95
Ricklefjärden	64.075	20.94167
Fårskäret	64.06667	20.94167
Skorvåsen	64.06667	21
Hebersgrundet	64.05833	20.98333
Tjyviken	64.05	20.93333
Lilla Hällgrund	64.05	20.96667
Östra Tärngrundet	64.04167	20.96667
Västra Tärngrundet	64.05	20.95
Lergrund	64.03333	20.96667
Borgarskäret	64.05	20.91667
Vitskäret	64.03333	20.93333
Vitskärsholmen	64.03333	20.93333
Hälsan	64.03333	20.9
Halsen	64	21.15
Östra Ryggen	64.03333	20.95
Västra Ryggen	64.03333	20.95
Ågrundet	64.03333	20.95
Bredgrundet	64.01667	20.94167
Dödmanskäret	64.01667	20.925
Dödmansudde	64.01667	20.91667
Rata Norrklubb	64	20.9
Storsanden	64	20.9
Lillgrundet	63.99167	20.91667
Båkskäret	63.99167	20.9
Lindbergsgrundet	63.98333	20.91667
Ledskärsviken	63.98333	20.89167
Blankhällan	63.98333	20.91667
Farstugrund	64	20.91667
Bullran	63.96667	20.9
Långrataudden	63.98333	20.88333
Djäknebodafjärden	63.98333	20.86667
Röjnäsudden	63.96667	20.85
Bådakroken	63.95	20.85
Sundskärsfjärden	63.95	20.83333
Svalphällan	63.94167	20.85
Nygrundet	63.9	20.8
Österbrottet	63.88333	20.8
Nidingen	63.86667	20.81667
Pysen	63.86667	20.81667
Ol-Nilsgrundet	63.86667	20.78333
Pynten	63.86667	21.03333
Karbasen	63.83333	20.98333
Utbrottet	63.83333	20.96667
Gubben	63.85	20.93333
Ellagrundet	63.85	20.9
Falkgrundet	63.85	20.88333
Anders-Ansgrundet	63.86667	20.76667
Norrfjärden	63.86667	20.73333
Truthälludden	63.85	20.73333
Pannan	63.85	20.78333
Stacken	63.84167	20.76667
Mittifjärden	63.85	20.73333
Laduskäret	63.85	20.71667
Lillgrundet	63.82386	21.00114
Betingen	63.81667	20.98333
Lilla Fjäderäggsgrundet	63.83333	20.95833
Björn	63.83333	20.93333
Skötholmen	63.81667	20.88333
Lillhällansgrunden	63.81667	20.85833
Galten	63.83333	20.86667
Svalgrundet	63.81667	20.8
Stänk-Revet	63.83333	20.73333
Kälsgrundet	63.825	20.725
Sandskäret	63.83333	20.68333
Svappeholmen	63.81424	21.00939
Själaholmen	63.80425	20.98015
Bänken	63.80833	20.975
Smonkgrundet	63.80833	20.95
Trappskäret	63.81667	20.91667
Röjbådan	63.80833	20.90833
Trapporna	63.81667	20.89167
Byviken	63.8	20.86667
Lillhällan	63.80833	20.86667
Tärngrunden	63.8	20.775
Utliggaren	63.81667	20.75833
Draken	63.8	20.73333
Oron	63.8	20.71667
Klubberget	63.81667	20.675
Svärtögern	63.8	20.93333
Långögern	63.79167	20.96667
Gåsflötan	63.78333	20.95
Långgrundet	63.78333	20.93333
Svärtesbredden	63.8	20.925
Bergudden	63.78333	20.83333
Roten	63.8	20.76667
Skvalpan	63.8	20.73333
Klimparna	63.78333	20.73333
Långbråten	63.78333	20.71667
Hästgataudden	63.8	20.66667
Malbådan	63.78333	20.975
Kungsängen	59.478	17.75294
Bro	59.5112	17.63645
Bålsta	59.56886	17.53285
Stuvsta	59.25303	17.99621
Huddinge	59.23692	17.97986
Tullinge	59.20516	17.90364
Tumba	59.1996	17.83622
Rönninge	59.19356	17.75
Järna	59.09361	17.56774
Mölnbo	59.04731	17.41809
Gnesta	59.04881	17.31093
Södertuna slott	59.06856	17.30531
Riksgränsen	68.42561	18.12641
Katterjåkk	68.41909	18.16237
Låktatjåkka	68.42358	18.32567
Abisko turiststation	68.35739	18.78244
Kiruna	67.86697	20.20068
Sjisjka	67.62426	20.23364
Kaitum	67.544	20.1073
Fjällåsen	67.5182	20.09352
Gällivare	67.13331	20.65095";
    }
}