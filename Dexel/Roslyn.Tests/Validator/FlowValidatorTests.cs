﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Roslyn.Validator.Tests
{
    [TestClass()]
    public class FlowValidatorTests
    {
        [TestMethod()]
        public void ValidateTest()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            var intOut = MainModelManager.AddNewOutput(main, "()");

            var trySomething = MainModelManager.AddNewFunctionUnit("try something", testModel);
            MainModelManager.AddNewInput(trySomething, "()");
            var OperationOut = MainModelManager.AddNewOutput(trySomething, "(string)", actionName: "onError");
            var OperationOut2 = MainModelManager.AddNewOutput(trySomething, "(string)", actionName: "onString");

            main.IsIntegrating.AddUnique(trySomething);

            List<ValidationErrorUnnconnectedOutput> errorsfound = new List<ValidationErrorUnnconnectedOutput>();
            FlowValidator.Validate(testModel, onErrorOrWarning: obj =>
            {
                errorsfound.Add((ValidationErrorUnnconnectedOutput) obj);
            });

            Assert.AreEqual(main, errorsfound[0].Integration);
            Assert.AreEqual(OperationOut, errorsfound[0].UnnconnectedOutput);

            Assert.AreEqual(main, errorsfound[1].Integration);
            Assert.AreEqual(OperationOut2, errorsfound[1].UnnconnectedOutput);


        }

        [TestMethod()]
        public void ValidateTest_UnusedVariableWarning()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            MainModelManager.AddNewOutput(main, "(int)");

            var op1 = MainModelManager.AddNewFunctionUnit("try something", testModel);
            MainModelManager.AddNewInput(op1, "()");
            var op1Out = MainModelManager.AddNewOutput(op1, "(string)");
            main.IsIntegrating.AddUnique(op1);

            var op2 = MainModelManager.AddNewFunctionUnit("try something", testModel);
            var op2In = MainModelManager.AddNewInput(op2, "()");
            var op2Out = MainModelManager.AddNewOutput(op2, "(int)");
            main.IsIntegrating.AddUnique(op2);

            MainModelManager.ConnectTwoDefintions(op1Out, op2In, testModel);

            List<ValidationWarningUnusedVariable> warningsFound = new List<ValidationWarningUnusedVariable>();
            FlowValidator.Validate(testModel, onErrorOrWarning: obj =>
            {
                warningsFound.Add((ValidationWarningUnusedVariable) obj);
            });
           
            Assert.AreEqual(op1Out, warningsFound[0].UnusedOutput);
            Assert.AreEqual(1, warningsFound.Count);
        }

        [TestMethod()]
        public void ValidateTest_UnusedVariableWarning_InsideConnected()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            MainModelManager.AddNewOutput(main, "(int)");

            var op1 = MainModelManager.AddNewFunctionUnit("try something", testModel);
            MainModelManager.AddNewInput(op1, "()");
            var op1Out = MainModelManager.AddNewOutput(op1, "(string, float)");
            main.IsIntegrating.AddUnique(op1);

            var op2 = MainModelManager.AddNewFunctionUnit("try something", testModel);
            var op2In = MainModelManager.AddNewInput(op2, "(float)");
            var op2Out = MainModelManager.AddNewOutput(op2, "(int)");
            main.IsIntegrating.AddUnique(op2);

            MainModelManager.ConnectTwoDefintions(op1Out, op2In, testModel);

            List<ValidationWarningUnusedVariable> warningsFound = new List<ValidationWarningUnusedVariable>();
            FlowValidator.Validate(testModel, onErrorOrWarning: obj =>
            {
                warningsFound.Add((ValidationWarningUnusedVariable) obj);
            });
            Assert.AreEqual(1, warningsFound.Count);
            Assert.AreEqual(op1Out, warningsFound[0].UnusedOutput);
           
        }



        [TestMethod()]
        public void ValidateTest_UnusedActionWarning()
        {
          
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            MainModelManager.AddNewOutput(main, "(int)");

            var op1 = MainModelManager.AddNewFunctionUnit("unnconnected action output", testModel);
            MainModelManager.AddNewInput(op1, "()");
            var op1Out = MainModelManager.AddNewOutput(op1, "(string)", "onString");
            main.IsIntegrating.AddUnique(op1);


            List<ValidationErrorUnnconnectedOutput> errors = new List<ValidationErrorUnnconnectedOutput>();
            FlowValidator.Validate(testModel, onErrorOrWarning: obj =>
            {
                errors.Add((ValidationErrorUnnconnectedOutput) obj);
            });
           
            Assert.AreEqual(op1Out, errors[0].UnnconnectedOutput);
            Assert.AreEqual(1, errors.Count);
        }

        [TestMethod()]
        public void ValidateTest_MissingInput()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            var intOut = MainModelManager.AddNewOutput(main, "()");

            var createname = MainModelManager.AddNewFunctionUnit("createname", testModel);
            MainModelManager.AddNewInput(createname, "()");
            var createnameOut = MainModelManager.AddNewOutput(createname, "(string)");


            var createPerson = MainModelManager.AddNewFunctionUnit("create Person", testModel);
            var createPersonIn =  MainModelManager.AddNewInput(createPerson, "(string,age:int)");
            MainModelManager.AddNewOutput(createPerson, "()");

            MainModelManager.ConnectTwoDefintions(createnameOut, createPersonIn, testModel);

            main.IsIntegrating.AddUnique(createname);
            main.IsIntegrating.AddUnique(createPerson);

            List<ValidationErrorInputMissing> errorsfound = new List<ValidationErrorInputMissing>();
            FlowValidator.Validate(testModel, onErrorOrWarning: obj =>
            {
                errorsfound.Add((ValidationErrorInputMissing) obj);
            });

            Assert.AreEqual(createPerson, errorsfound[0].invalidFunctionUnit);
            Assert.AreEqual("age", errorsfound[0].notFoundNameType.Name);
            Assert.AreEqual("int", errorsfound[0].notFoundNameType.Type);



        }
    }
}