﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.io.tests {

    public class TestPlayerPrefsV2 {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestGetAndSetBool() {
            var key = "b1";
            Assert.IsFalse(PlayerPrefsV2.GetBool(key, false));
            PlayerPrefsV2.SetBool(key, true);
            Assert.IsTrue(PlayerPrefsV2.GetBool(key, false));
            PlayerPrefsV2.DeleteKey(key);
        }

        [Test]
        public void TestGetAndSetEncyptedString() {
            var key = "b1";
            var value = "val 1";
            var password = "1234";
            Assert.AreEqual(null, PlayerPrefsV2.GetStringDecrypted(key, null, password));
            PlayerPrefsV2.SetStringEncrypted(key, value, password);
            Assert.AreEqual(value, PlayerPrefsV2.GetStringDecrypted(key, null, password));
            Assert.AreNotEqual(value, PlayerPrefsV2.GetStringDecrypted(key, null, "incorrect password"));
            Assert.AreNotEqual(value, PlayerPrefsV2.GetString(key, null));
            PlayerPrefsV2.DeleteKey(key);
        }

    }

}
