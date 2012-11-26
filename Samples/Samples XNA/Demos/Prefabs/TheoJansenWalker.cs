﻿#region Using System
using System;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos.Prefabs
{
  public class TheoJansenWalker
  {
    private Body _chassis;
    private Body _wheel;
    private Body[] _leftShoulders;
    private Body[] _leftLegs;
    private Body[] _rightShoulders;
    private Body[] _rightLegs;

    private Sprite _body;
    private Sprite _engine;
    private Sprite _leftShoulder;
    private Sprite _leftLeg;
    private Sprite _rightShoulder;
    private Sprite _rightLeg;

    private RevoluteJoint _motorJoint;
    private List<DistanceJoint> _walkerJoints = new List<DistanceJoint>();

    private bool _motorOn;
    private float _motorSpeed;

    private Vector2 _position;

    public TheoJansenWalker(World world, Vector2 position)
    {
      _position = position;
      _motorSpeed = 2.0f;
      _motorOn = true;

      _leftShoulders = new Body[3];
      _leftLegs = new Body[3];

      _rightShoulders = new Body[3];
      _rightLegs = new Body[3];

      Vector2 pivot = new Vector2(0f, -0.8f);

      // Chassis
      PolygonShape box = new PolygonShape(1f);
      box.Vertices = PolygonTools.CreateRectangle(2.5f, 1.0f);

      _body = new Sprite(AssetCreator.TextureFromShape(box, AssetCreator.Sky, AssetCreator.Black));

      _chassis = BodyFactory.CreateBody(world);
      _chassis.BodyType = BodyType.Dynamic;
      _chassis.Position = pivot + _position;

      Fixture bodyFixture = _chassis.CreateFixture(box);
      bodyFixture.CollisionGroup = -1;

      // Wheel
      CircleShape circle = new CircleShape(1.6f, 1f);
      _engine = new Sprite(AssetCreator.TextureFromShape(circle, "stripe", AssetCreator.Sky, AssetCreator.Sunset, AssetCreator.Black, 3f));

      _wheel = BodyFactory.CreateBody(world);
      _wheel.BodyType = BodyType.Dynamic;
      _wheel.Position = pivot + _position;

      Fixture wheelFixture = _wheel.CreateFixture(circle);
      wheelFixture.CollisionGroup = -1;

      // Physics
      _motorJoint = new RevoluteJoint(_wheel, _chassis.Position, _chassis, Vector2.Zero);
      _motorJoint.CollideConnected = false;
      _motorJoint.MotorSpeed = _motorSpeed;
      _motorJoint.MaxMotorTorque = 400f;
      _motorJoint.MotorEnabled = _motorOn;
      world.AddJoint(_motorJoint);

      Vector2 wheelAnchor = pivot + new Vector2(0f, 0.8f);

      CreateLeg(world, -1f, wheelAnchor, 0);
      CreateLeg(world, 1f, wheelAnchor, 0);

      _wheel.SetTransform(_wheel.Position, 120f * Settings.Pi / 180f);
      CreateLeg(world, -1f, wheelAnchor, 1);
      CreateLeg(world, 1f, wheelAnchor, 1);

      _wheel.SetTransform(_wheel.Position, -120f * Settings.Pi / 180f);
      CreateLeg(world, -1f, wheelAnchor, 2);
      CreateLeg(world, 1f, wheelAnchor, 2);

      // GFX
      Vector2[] points = { 
        new Vector2(-5.4f, 6.1f),
        new Vector2(-7.2f, 1.2f),
        new Vector2(-4.3f, 1.9f),
        new Vector2(-2.9f, -0.7f),
        new Vector2(0.6f, -2.9f)
      };

      _leftShoulder = new Sprite(AssetCreator.PolygonTexture(new Vector2[] { Vector2.Zero, points[3], points[4] }, AssetCreator.Sky, AssetCreator.Black));
      _leftShoulder.Origin = AssetCreator.CalculateOrigin(_leftShoulders[0]);

      _leftLeg = new Sprite(AssetCreator.PolygonTexture(new Vector2[] { points[0], points[1], points[2] }, AssetCreator.Sky, AssetCreator.Black));
      _leftLeg.Origin = AssetCreator.CalculateOrigin(_leftLegs[0]);

      for (int i = 0; i < points.Length; i++)
      {
        points[i] *= -1f;
      }

      _rightShoulder = new Sprite(AssetCreator.PolygonTexture(new Vector2[] { Vector2.Zero, points[4], points[3] }, AssetCreator.Sky, AssetCreator.Black));
      _rightShoulder.Origin = AssetCreator.CalculateOrigin(_rightShoulders[0]);

      _rightLeg = new Sprite(AssetCreator.PolygonTexture(new Vector2[] { points[0], points[2], points[1] }, AssetCreator.Sky, AssetCreator.Black));
      _rightLeg.Origin = AssetCreator.CalculateOrigin(_rightLegs[0]);
    }

    private void CreateLeg(World world, float direction, Vector2 wheelAnchor, int index)
    {
      Vector2[] points = {
        new Vector2(5.4f * direction, 6.1f),
        new Vector2(7.2f * direction, 1.2f),
        new Vector2(4.3f * direction, 1.9f),
        new Vector2(3.1f * direction, -0.8f),
        new Vector2(6.0f * direction, -1.5f),
        new Vector2(2.5f * direction, -3.7f)
      };


      PolygonShape legPolygon = new PolygonShape(1f);
      PolygonShape shoulderPolygon = new PolygonShape(2f);

      if (direction < 0f)
      {
        legPolygon.Vertices.AddRange(new Vector2[] { points[0], points[1], points[2] });
        shoulderPolygon.Vertices.AddRange(new Vector2[] { Vector2.Zero, points[4] - points[3], points[5] - points[4] });
      }
      if (direction > 0f)
      {
        legPolygon.Vertices.AddRange(new Vector2[] { points[0], points[2], points[1] });
        shoulderPolygon.Vertices.AddRange(new Vector2[] { Vector2.Zero, points[5] - points[3], points[4] - points[3] });
      }

      Body leg = BodyFactory.CreateBody(world);
      leg.BodyType = BodyType.Dynamic;
      leg.Position = _position;
      leg.AngularDamping = 10f;
      if (direction < 0f)
      {
        _leftLegs[index] = leg;
      }
      if (direction > 0f)
      {
        _rightLegs[index] = leg;
      }

      Body shoulder = BodyFactory.CreateBody(world);
      shoulder.BodyType = BodyType.Dynamic;
      shoulder.Position = points[3] + _position;
      shoulder.AngularDamping = 10f;
      if (direction < 0f)
      {
        _leftShoulders[index] = shoulder;
      }
      if (direction > 0f)
      {
        _rightShoulders[index] = shoulder;
      }

      Fixture legFixture = leg.CreateFixture(legPolygon);
      legFixture.CollisionGroup = -1;

      Fixture shoulderFixture = shoulder.CreateFixture(shoulderPolygon);
      shoulderFixture.CollisionGroup = -1;

      // Using a soft distancejoint can reduce some jitter.
      // It also makes the structure seem a bit more fluid by
      // acting like a suspension system.
      DistanceJoint djd = new DistanceJoint(leg, shoulder, points[1] + _position, points[4] + _position);
      djd.DampingRatio = 0.5f;
      djd.Frequency = 10f;

      world.AddJoint(djd);
      _walkerJoints.Add(djd);

      DistanceJoint djd2 = new DistanceJoint(leg, shoulder, points[2] + _position, points[3] + _position);
      djd2.DampingRatio = 0.5f;
      djd2.Frequency = 10f;

      world.AddJoint(djd2);
      _walkerJoints.Add(djd2);

      DistanceJoint djd3 = new DistanceJoint(leg, _wheel, points[2] + _position, wheelAnchor + _position);
      djd3.DampingRatio = 0.5f;
      djd3.Frequency = 10f;

      world.AddJoint(djd3);
      _walkerJoints.Add(djd3);

      DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, points[5] + _position, wheelAnchor + _position);
      djd4.DampingRatio = 0.5f;
      djd4.Frequency = 10f;

      world.AddJoint(djd4);
      _walkerJoints.Add(djd4);

      Vector2 anchor = points[3] - new Vector2(0f, -0.8f);
      RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis.GetWorldPoint(anchor), _chassis, anchor);
      world.AddJoint(rjd);
    }

    public void Reverse()
    {
      _motorSpeed *= -1f;
      _motorJoint.MotorSpeed = _motorSpeed;
    }

    public void Draw(SpriteBatch batch, LineBatch lines, Camera2D camera)
    {
      batch.Begin(0, null, null, null, null, null, camera.View);
      batch.Draw(_body.Image, ConvertUnits.ToDisplayUnits(_chassis.Position), null, Color.White, _chassis.Rotation, _body.Origin, 1f, SpriteEffects.None, 0f);
      for (int i = 0; i < 3; ++i)
      {
        batch.Draw(_leftLeg.Image, ConvertUnits.ToDisplayUnits(_leftLegs[i].Position), null, Color.White, _leftLegs[i].Rotation, _leftLeg.Origin, 1f, SpriteEffects.None, 0f);
        batch.Draw(_leftShoulder.Image, ConvertUnits.ToDisplayUnits(_leftShoulders[i].Position), null, Color.White, _leftShoulders[i].Rotation, _leftShoulder.Origin, 1f, SpriteEffects.None, 0f);
        batch.Draw(_rightLeg.Image, ConvertUnits.ToDisplayUnits(_rightLegs[i].Position), null, Color.White, _rightLegs[i].Rotation, _rightLeg.Origin, 1f, SpriteEffects.None, 0f);
        batch.Draw(_rightShoulder.Image, ConvertUnits.ToDisplayUnits(_rightShoulders[i].Position), null, Color.White, _rightShoulders[i].Rotation, _rightShoulder.Origin, 1f, SpriteEffects.None, 0f);
      }
      batch.Draw(_engine.Image, ConvertUnits.ToDisplayUnits(_wheel.Position), null, Color.White, _wheel.Rotation, _engine.Origin, 1f, SpriteEffects.None, 0f);
      batch.End();

      lines.Begin(camera.SimProjection, camera.SimView);
      for (int i = 0; i < _walkerJoints.Count; ++i)
      {
        lines.DrawLine(_walkerJoints[i].WorldAnchorA, _walkerJoints[i].WorldAnchorB, AssetCreator.Grey);
      }
      lines.End();
    }
  }
}