﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.PCA;
using Microsoft.ML.Runtime.RunTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.ML.Tests.TrainerEstimators
{
    public partial class TrainerEstimators : TestDataPipeBase
    {
        public TrainerEstimators(ITestOutputHelper helper) : base(helper)
        {
        }

        /// <summary>
        /// FastTreeBinaryClassification TrainerEstimator test 
        /// </summary>
        [Fact]
        public void PCATrainerEstimator()
        {
            string featureColumn = "NumericFeatures";

            var reader = new TextLoader(Env, new TextLoader.Arguments()
            {
                HasHeader = true,
                Separator = "\t",
                Column = new[]
                {
                    new TextLoader.Column(featureColumn, DataKind.R4, new [] { new TextLoader.Range(1, 784) })
                }
            });
            var data = reader.Read(new MultiFileSource(GetDataPath(TestDatasets.mnistOneClass.trainFilename)));


            // Pipeline.
            var pipeline = new RandomizedPcaTrainer(Env, featureColumn, rank:10);

            TestEstimatorCore(pipeline, data);
            Done();
        }

        /// <summary>
        /// KMeans TrainerEstimator test 
        /// </summary>
        [Fact]
        public void KMeansEstimator()
        {
            string featureColumn = "NumericFeatures";
            string weights = "Weights";

            var reader = new TextLoader(Env, new TextLoader.Arguments
            {
                HasHeader = true,
                Separator = "\t",
                Column = new[]
                {
                    new TextLoader.Column(featureColumn, DataKind.R4, new [] { new TextLoader.Range(1, 784) }),
                    new TextLoader.Column(weights, DataKind.R4, 0)
                }
            });
            var data = reader.Read(new MultiFileSource(GetDataPath(TestDatasets.mnistTiny28.trainFilename)));


            // Pipeline.
            var pipeline = new KMeansPlusPlusTrainer(Env, featureColumn, weightColumn: weights,
                            advancedSettings: s => { s.InitAlgorithm = KMeansPlusPlusTrainer.InitAlgorithm.KMeansParallel; });

            TestEstimatorCore(pipeline, data);

            Done();
        }

        /// <summary>
        /// HogwildSGD TrainerEstimator test 
        /// </summary>
        [Fact]
        public void TestEstimatorHogwildSGD()
        {
            (IEstimator<ITransformer> pipe, IDataView dataView) = GetBinaryClassificationPipeline();
            pipe.Append(new StochasticGradientDescentClassificationTrainer(Env, "Features", "Label"));
            TestEstimatorCore(pipe, dataView);
            Done();
        }

        /// <summary>
        /// MultiClassNaiveBayes TrainerEstimator test 
        /// </summary>
        [Fact]
        public void TestEstimatorMultiClassNaiveBayesTrainer()
        {
            (IEstimator<ITransformer> pipe, IDataView dataView) = GetMultiClassPipeline();
            pipe.Append(new MultiClassNaiveBayesTrainer(Env, "Features", "Label"));
            TestEstimatorCore(pipe, dataView);
            Done();
        }

        private (IEstimator<ITransformer>, IDataView) GetBinaryClassificationPipeline()
        {
            var data = new TextLoader(Env,
                    new TextLoader.Arguments()
                    {
                        Separator = "\t",
                        HasHeader = true,
                        Column = new[]
                        {
                            new TextLoader.Column("Label", DataKind.BL, 0),
                            new TextLoader.Column("SentimentText", DataKind.Text, 1)
                        }
                    }).Read(new MultiFileSource(GetDataPath(TestDatasets.Sentiment.trainFilename)));

            // Pipeline.
            var pipeline = new TextTransform(Env, "SentimentText", "Features");

            return (pipeline, data);
        }


        private (IEstimator<ITransformer>, IDataView) GetRankingPipeline()
        {
            var data = new TextLoader(Env, new TextLoader.Arguments
            {
                HasHeader = true,
                Separator = "\t",
                Column = new[]
                     {
                        new TextLoader.Column("Label", DataKind.R4, 0),
                        new TextLoader.Column("Workclass", DataKind.Text, 1),
                        new TextLoader.Column("NumericFeatures", DataKind.R4, new [] { new TextLoader.Range(9, 14) })
                    }
            }).Read(new MultiFileSource(GetDataPath(TestDatasets.adultRanking.trainFilename)));

            // Pipeline.
            var pipeline = new TermEstimator(Env, new[]{
                                    new TermTransform.ColumnInfo("Workclass", "Group"),
                                    new TermTransform.ColumnInfo("Label", "Label0") });

            return (pipeline, data);
        }

        private IDataView GetRegressionPipeline()
        {
            return new TextLoader(Env,
                    new TextLoader.Arguments()
                    {
                        Separator = ";",
                        HasHeader = true,
                        Column = new[]
                        {
                            new TextLoader.Column("Label", DataKind.R4, 11),
                            new TextLoader.Column("Features", DataKind.R4, new [] { new TextLoader.Range(0, 10) } )
                        }
                    }).Read(new MultiFileSource(GetDataPath(TestDatasets.generatedRegressionDatasetmacro.trainFilename)));
        }

        private TextLoader.Arguments GetIrisLoaderArgs()
        {
            return new TextLoader.Arguments()
            {
                Separator = "comma",
                HasHeader = true,
                Column = new[]
                        {
                            new TextLoader.Column("Features", DataKind.R4, new [] { new TextLoader.Range(0, 3) }),
                            new TextLoader.Column("Label", DataKind.Text, 4)
                        }
            };
        }

        private (IEstimator<ITransformer>, IDataView) GetMultiClassPipeline()
        {

            var data = new TextLoader(Env, new TextLoader.Arguments()
            {
                Separator = "comma",
                HasHeader = true,
                Column = new[]
                        {
                            new TextLoader.Column("Features", DataKind.R4, new [] { new TextLoader.Range(0, 3) }),
                            new TextLoader.Column("Label", DataKind.Text, 4)
                        }
                })
                .Read(new MultiFileSource(GetDataPath(IrisDataPath)));

            var pipeline = new TermEstimator(Env, "Label");

            return (pipeline, data);
        }
    }
}