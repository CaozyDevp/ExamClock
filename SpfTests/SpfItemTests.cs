/*
 * Copyright(C) 2025 Zachary Cao (CaozyDevp)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Spf;

namespace SpfTests;

[TestClass]
public class SpfItemTests
{
    [TestMethod]
    public void GetString_Test()
    {
        var profileItem = new ConfigurationItem()
        {
            Key = "PROFILE",
            Properties = new List<PropertyItem>()
                {
                    new PropertyItem("name", "Zachary"),    // string
                    new PropertyItem("age", 16),            // int
                    new PropertyItem("valid", true),        // bool
                    new PropertyItem("id", (ushort)5683),   // ushort
                    new PropertyItem("birth", new DateTime(2025,8,21))  // DateTime
                }
        };
        var scoreItem = new ConfigurationItem()
        {
            Key = "SCORES",
            Properties = new List<PropertyItem>()
                {
                    new PropertyItem("cn", 119.5),          // double
                    new PropertyItem("en", 115.5),          // double
                    new PropertyItem("mt", 110.5)           // double
                }
        };
        List<ConfigurationItem> items = new List<ConfigurationItem>()
        {
            profileItem, scoreItem
        };

        string tempResult = new SpfItem()
        {
            Configuration = items
        }.GetString();
        string result = "";
        foreach (var c in tempResult)
        {
            if (char.IsWhiteSpace(c)) continue;
            result += c;
        }

        string tempExpected = "#PROFILE{name = string:\"Zachary\",age = int:\"16\",valid = bool:\"True\",id = ushort:\"5683\",birth = DateTime:\"2025/8/21 0:0:0.0\"}" +
            "#SCORES{cn = double:\"119.5\",en = double:\"115.5\",mt = double:\"110.5\"}";
        string expected = "";
        foreach (var c in tempExpected)
        {
            if (char.IsWhiteSpace(c)) continue;
            expected += c;
        }

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void GetValue_Test()
    {
        var profileItem = new ConfigurationItem()
        {
            Key = "PROFILE",
            Properties = new List<PropertyItem>()
                {
                    new PropertyItem("name", "Zachary"),    // string
                    new PropertyItem("age", 16),            // int
                    new PropertyItem("valid", true),        // bool
                    new PropertyItem("id", (ushort)5683),   // ushort
                    new PropertyItem("birth", new DateTime(2025,8,21))  // DateTime
                }
        };
        var scoreItem = new ConfigurationItem()
        {
            Key = "SCORES",
            Properties = new List<PropertyItem>()
                {
                    new PropertyItem("cn", 119.5),          // double
                    new PropertyItem("en", 115.5),          // double
                    new PropertyItem("mt", 110.5)           // double
                }
        };
        List<ConfigurationItem> items = new List<ConfigurationItem>()
        {
            profileItem, scoreItem
        };

        string expectedName = "Zachary";
        var resultName = new SpfItem()
        {
            Configuration = items
        }.GetValue("PROFILE.name");

        Assert.AreEqual(expectedName.ToString(), resultName.ToString());
    }

    [TestMethod]
    [DataRow("PROFILE.name", true)]
    [DataRow("PROFILE.age", true)]
    [DataRow("PROFILE.home", false)]
    public void CheckIfExists_Test(string path, bool expected)
    {
        var spf = new SpfItem()
        {
            Configuration = new List<ConfigurationItem>()
            {
                new ConfigurationItem()
                {
                    Key = "PROFILE",
                    Properties = new List<PropertyItem>()
                    {
                        new PropertyItem("name", "Zachary"),
                        new PropertyItem("age", 16)
                    }
                }
            }
        };

        var result = spf.CheckIfExists(path);

        Assert.AreEqual(expected, result);
    }
}
