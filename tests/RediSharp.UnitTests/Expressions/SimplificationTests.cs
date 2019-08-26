using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;

namespace RediSharp.UnitTests.Expressions
{
    [TestClass]
    public class SimplificationTests
    {
        [TestMethod]
        public void ShouldNotSimplifyIdEqualsNil()
        {
            var someId = new IdentifierNode("someId", DataValueType.String);
            var expr = someId == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.Equal, someId, ExpressionNode.Nil));
        }

        [TestMethod]
        public void ShouldSimplifyConstantEqualsNil_1()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var expr = someConst == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Boolean, false));
        }

        [TestMethod]
        public void ShouldSimplifyConstantEqualsNil_2()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var expr = someConst != ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Boolean, true)); 
        }

        [TestMethod]
        public void ShouldSimplifyLogical_1()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var someId = new IdentifierNode("someId", DataValueType.String);
            var expr = someConst == ExpressionNode.Nil && someId == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Boolean, false));
        }
        
        [TestMethod]
        public void ShouldSimplifyLogical_2()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var someId = new IdentifierNode("someId", DataValueType.String);
            var expr = someConst != ExpressionNode.Nil || someId == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Boolean, true));
        }
        
        [TestMethod]
        public void ShouldSimplifyLogical_3()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var someId = new IdentifierNode("someId", DataValueType.String);
            var expr = someConst == ExpressionNode.Nil || someId == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(someId == ExpressionNode.Nil);
        }
        
        [TestMethod]
        public void ShouldSimplifyLogical_4()
        {
            var someConst = new ConstantValueNode(DataValueType.String, "abc");
            var someId = new IdentifierNode("someId", DataValueType.String);
            var expr = someConst != ExpressionNode.Nil && someId == ExpressionNode.Nil;
            expr.Should().BeEquivalentTo(someId == ExpressionNode.Nil);
        }
    }
}